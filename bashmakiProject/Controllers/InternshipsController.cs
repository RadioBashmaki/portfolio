using System.Security.Claims;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Driver;

namespace bashmakiProject.Controllers;

[Route("[controller]")]
public class InternshipsController : Controller
{
    private readonly IMongoDbRepository _repository;

    public InternshipsController(IMongoDbRepository repo)
    {
        _repository = repo;
    }

    [HttpGet("new")]
    [Authorize(Policy = "OnlyForRepresentatives")]
    public IActionResult CreateInternship()
    {
        return View();
    }

    [HttpPost("new")]
    [Authorize(Policy = "OnlyForRepresentatives")]
    public async Task<IActionResult> CreateInternship([FromForm] CreateInternshipRequest request)
    {
        if (!ModelState.IsValid)
        {
            var keys = ModelState.Keys.Where(x => x.EndsWith("File")).ToArray();
            for (var i = 0; i < keys.Length; i++)
                ModelState.AddModelError("", $"Отсутствует файл {i + 1}");
            return View(request);
        }

        var user = await GetCurrentUser();
        var internshipsCollection = _repository.GetCollection<Internship>();
        var internship = new Internship
        {
            UserId = user.Id,
            Title = request.Title,
            Description = request.Description,
            Experience = request.Experience!.Value,
            IsActive = request.IsActive,
            CreationDate = DateTime.Today,
            CompanyName = user.PersonalData.Company?.Name,
            Topics = request.Topics.Keys.Where(x => request.Topics[x]).ToArray(),
            Files = request.FilesDescriptions?.Select(desc => new FileDescriptionDatabase
            {
                Name = desc.Name,
                Description = desc.Description,
                File = FormFileToByteArray(desc.File).Result,
                ContentType = desc.File.ContentType,
                Extension = Path.GetExtension(desc.File.FileName)
            }).ToArray(),
        };
        await internshipsCollection.InsertOneAsync(internship);
        return RedirectToAction("Wall", "Home", new { id = internship.UserId });
    }

    [Authorize(Policy = "OnlyForRepresentatives")]
    [HttpPost("newFile")]
    public IActionResult AddFile([Bind("FilesDescriptions")] CreateInternshipRequest createRequest)
    {
        ViewData["index"] = createRequest.FilesDescriptions?.Length ?? 0;
        return PartialView("_ModalPartial");
    }

    [Authorize(Policy = "OnlyForRepresentatives")]
    [HttpGet("{id}/edit")]
    public async Task<IActionResult> EditInternship([FromRoute] string id)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return NotFound();
        var user = await GetCurrentUser();
        var internshipsCollection = _repository.GetCollection<Internship>();
        var filter = Builders<Internship>.Filter
            .Eq(i => i.Id, id);
        var currentInternship = await internshipsCollection.Find(filter).FirstOrDefaultAsync();
        if (currentInternship == null)
            return NotFound();
        if (currentInternship.UserId != user.Id)
            return Forbid();
        ViewData["internshipId"] = currentInternship.Id;
        ViewData["internshipTitle"] = currentInternship.Title;

        var files = new List<EditProjectFileDescription>();
        if (currentInternship.Files != null)
            files.AddRange(currentInternship.Files
                .Select(desc => new EditProjectFileDescription
                {
                    DatabaseId = desc.Id, Name = desc.Name, Description = desc.Description, Extension = desc.Extension
                }));
        var edit = new EditInternshipRequest
        {
            Title = currentInternship.Title,
            Description = currentInternship.Description,
            FilesDescriptions = files.ToArray(),
            Topics = Enum.GetValues<Topic>().ToDictionary(x => x, y => currentInternship.Topics.Contains(y))
        };

        return View(edit);
    }

    [Authorize(Policy = "OnlyForRepresentatives")]
    [HttpPost("{id}/edit")]
    public async Task<IActionResult> EditInternship([FromRoute] string id, [FromForm] EditInternshipRequest editRequest)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return NotFound();
        var user = await GetCurrentUser();
        var internshipsCollection = _repository.GetCollection<Internship>();
        var filter = Builders<Internship>.Filter
            .Eq(i => i.Id, id);
        var currentInternship = await internshipsCollection.Find(filter).FirstOrDefaultAsync();
        if (currentInternship == null)
            return NotFound();
        if (currentInternship.UserId != user.Id)
            return Forbid();
        if (!ModelState.IsValid)
        {
            var errorsWithFiles = ModelState.Count(item =>
                item.Key.EndsWith("File") && item.Value.ValidationState == ModelValidationState.Invalid);
            if (errorsWithFiles != ModelState.ErrorCount)
                return View(editRequest);
        }

        var editedFiles = editRequest.FilesDescriptions?.Where(x => x.DatabaseId != null)
            .ToDictionary(x => x.DatabaseId!);

        currentInternship.Title = editRequest.Title;
        currentInternship.Description = editRequest.Description;
        currentInternship.Topics = editRequest.Topics.Keys.Where(key => editRequest.Topics[key]).ToArray();
        currentInternship.IsActive = editRequest.IsActive;
        if (editRequest.FilesDescriptions == null || editRequest.FilesDescriptions.Length == 0 ||
            editedFiles == null || editedFiles.Count == 0)
            currentInternship.Files = null;
        else if (currentInternship.Files != null)
        {
            var oldFiles = currentInternship.Files.ToDictionary(x => x.Id);
            var filesLeft = editedFiles.Keys.Select(key =>
            {
                var newFile = new FileDescriptionDatabase
                {
                    Name = editedFiles[key].Name,
                    Description = editedFiles[key].Description
                };
                if (editedFiles[key].File != null)
                {
                    newFile.File = FormFileToByteArray(editedFiles[key].File!).Result;
                    newFile.Extension = Path.GetExtension(editedFiles[key].File!.FileName);
                    newFile.ContentType = editedFiles[key].File!.ContentType;
                }
                else
                {
                    newFile.File = oldFiles[key].File;
                    newFile.Extension = oldFiles[key].Extension;
                    newFile.ContentType = oldFiles[key].ContentType;
                }

                return newFile;
            });
            currentInternship.Files = filesLeft.ToArray();
        }

        var filesList = currentInternship.Files?.ToList() ?? new List<FileDescriptionDatabase>();
        foreach (var newFile in editRequest.FilesDescriptions?.Where(f => f.DatabaseId == null) ??
                                Array.Empty<EditProjectFileDescription>())
            filesList.Add(new FileDescriptionDatabase
            {
                Name = newFile.Name,
                Description = newFile.Description,
                File = await FormFileToByteArray(newFile.File!),
                ContentType = newFile.File!.ContentType,
                Extension = Path.GetExtension(newFile.File!.FileName)
            });
        currentInternship.Files = filesList.ToArray();
        await internshipsCollection.ReplaceOneAsync(filter, currentInternship);
        return RedirectToAction("Wall", "Home", new { id = currentInternship.UserId });
    }

    [NonAction]
    private async Task<User> GetCurrentUser()
    {
        var collection = _repository.GetCollection<User>();
        var mail = User.FindFirstValue(ClaimTypes.Email);
        var filter = Builders<User>.Filter
            .Eq(u => u.Email, mail);
        var user = await collection
            .Find(filter)
            .FirstOrDefaultAsync();
        return user;
    }

    [NonAction]
    private async Task<byte[]> FormFileToByteArray(IFormFile formFile)
    {
        using var ms = new MemoryStream();
        await formFile.OpenReadStream().CopyToAsync(ms);
        return ms.ToArray();
    }
}