using System.Security.Claims;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace bashmakiProject.Controllers;

[Route("[controller]")]
public class ProjectsController : Controller
{
    private readonly IMongoDbRepository _repository;

    public ProjectsController(IMongoDbRepository repository)
    {
        _repository = repository;
    }

    [AcceptVerbs("GET", "POST")]
    [Route("new")]
    [Authorize(Policy = "OnlyForStudents")]
    public async Task<IActionResult> NewProject([FromForm] CreateProjectRequest createProjectRequest)
    {
        if (Request.Method == "GET")
        {
            foreach (var field in ModelState.Keys)
                ModelState.ClearValidationState(field);
            return View();
        }

        if (!ModelState.IsValid)
        {
            var keys = ModelState.Keys.Where(x => x.EndsWith("File")).ToArray();
            for (var i = 0; i < keys.Length; i++)
                ModelState.AddModelError("", $"Отсутствует файл {i + 1}");
            return View(createProjectRequest);
        }

        var project = await GetProject(createProjectRequest);
        await _repository.GetCollection<Project>().InsertOneAsync(project);

        return RedirectToAction("Index", "Home");
    }

    private async Task<Project> GetProject(CreateProjectRequest createProjectRequest)
    {
        var user = await _repository.GetCollection<User>()
            .Find(u => u.Email == User.FindFirst(ClaimTypes.Email)!.Value)
            .FirstOrDefaultAsync()!;

        List<FileDescriptionDatabase>? files = null;
        if (createProjectRequest.FilesDescriptions != null)
        {
            files = new List<FileDescriptionDatabase>();
            foreach (var clientFile in createProjectRequest.FilesDescriptions)
            {
                files.Add(new FileDescriptionDatabase
                {
                    Name = clientFile.Name,
                    Description = clientFile.Description,
                    File = await FormFileToByteArray(clientFile.File)
                });
            }
        }

        var project = new Project
        {
            UserId = user.Id,
            Title = createProjectRequest.Title,
            Description = createProjectRequest.Description,
            Topics = createProjectRequest.Topics.Keys.Where(key => createProjectRequest.Topics[key]).ToArray(),
            CreationDate = DateTime.Now.Date,
            Files = files?.ToArray()
        };

        return project;
    }

    [AcceptVerbs("GET")]
    [Route("addFileFormModal")]
    [Authorize(Policy = "OnlyForStudents")]
    public IActionResult ModalPartial(int index)
    {
        ViewData["index"] = index.ToString();
        return PartialView("_ModalPartial");
    }

    [NonAction]
    private async Task<byte[]> FormFileToByteArray(IFormFile formFile)
    {
        using var ms = new MemoryStream();
        await formFile.OpenReadStream().CopyToAsync(ms);
        return ms.ToArray();
    }
}