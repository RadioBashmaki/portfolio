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
        return View(request);
    }

    [Authorize(Policy = "OnlyForRepresentatives")]
    [HttpPost("newFile")]
    public IActionResult AddFile([Bind("FilesDescriptions")] CreateInternshipRequest createRequest)
    {
        ViewData["index"] = createRequest.FilesDescriptions?.Length ?? 0;
        return PartialView("_AddFilePartial");
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