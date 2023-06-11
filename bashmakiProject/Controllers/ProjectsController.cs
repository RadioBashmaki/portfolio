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
        var user = await GetCurrentUser();

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
                    File = await FormFileToByteArray(clientFile.File),
                    ContentType = clientFile.File.ContentType,
                    Extension = Path.GetExtension(clientFile.File.FileName)
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

    [AcceptVerbs("GET")]
    [Route("")]
    [Authorize(Policy = "OnlyForStudents")]
    public async Task<IActionResult> MyProjects()
    {
        var user = await GetCurrentUser();
        var projectsCollection = _repository.GetCollection<Project>();
        var filter = Builders<Project>.Filter
            .Eq(proj => proj.UserId, user.Id);
        var projects = await projectsCollection.Find(filter).ToListAsync();
        var model = new FilterProjectsRequest
        {
            Projects = projects
        };

        return View(model);
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
    
    [AcceptVerbs("GET", "POST")]
    [Route("{id}/edit")]
    [Authorize(Policy = "OnlyForStudents")]
    public async Task<IActionResult> EditProject([FromRoute] string id)
    {
        var user = await GetCurrentUser();
        var projectsCollection = _repository.GetCollection<Project>();
        var filter = Builders<Project>.Filter
            .Eq(proj => proj.UserId, user.Id);
        var projects = await projectsCollection.Find(filter).ToListAsync();
        if (!projects.Select(proj => proj.Id).Contains(id))
            return Forbid();
        
        return View();
    }

    [AcceptVerbs("POST")]
    [Route("filter")]
    [Authorize(Policy = "OnlyForStudents")]
    public async Task<IActionResult> FilterProjects([FromForm][Bind("Topics", "ComparisonString")] FilterProjectsRequest filterProjectsRequest)
    {
        var user = await GetCurrentUser();
        var projectsCollection = _repository.GetCollection<Project>();
        var filter = Builders<Project>.Filter
            .Eq(proj => proj.UserId, user.Id);
        var allProjects = await projectsCollection.Find(filter).ToListAsync();
        filterProjectsRequest.Projects = allProjects
            .Where(proj =>
            {
                return proj.Title.Contains(filterProjectsRequest.ComparisonString ?? "") &&
                       proj.Topics.Intersect(filterProjectsRequest.Topics.Keys.
                           Where(key => filterProjectsRequest.Topics[key]))
                           .Any();
            }).ToList();
        return PartialView("_FilterProjectsPartial", filterProjectsRequest);
    }

    [AcceptVerbs("POST")]
    [Route("pin")]
    [Authorize(Policy = "OnlyForStudents")]
    public async Task<IActionResult> ToggleProjectsPin(string id)
    {
        var user = await GetCurrentUser();
        var projectsCollection = _repository.GetCollection<Project>();
        var filter = Builders<Project>.Filter.Eq(proj => proj.Id, id);
        var project = await projectsCollection.Find(filter).FirstOrDefaultAsync();
        if (project.UserId != user.Id)
            return Forbid();
        var update = Builders<Project>.Update
            .Set(proj => proj.IsPinned, !project.IsPinned);
        var result = await projectsCollection.UpdateOneAsync(filter, update);
        return Json(new {
            successful = result.IsAcknowledged,
            pinned = !project.IsPinned,
            projectTitle = project.Title,
        });
    }
}