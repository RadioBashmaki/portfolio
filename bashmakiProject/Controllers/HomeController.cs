using System.Security.Claims;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace bashmakiProject.Controllers;

public class HomeController : Controller
{
    private readonly IMongoDbRepository _repository;

    public HomeController(IMongoDbRepository repo)
    {
        _repository = repo;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public IActionResult Index()
    {
        ViewBag.User = User;
        return View();
    }

    [HttpGet("wall/{id?}")]
    public async Task<IActionResult> Wall(string? id)
    {
        if (id != null && !MongoDB.Bson.ObjectId.TryParse(id, out _))
            return NotFound();
        var currentUser = await GetCurrentUser();
        var user = id == null
            ? currentUser
            : await _repository.GetCollection<User>().Find(u => u.Id == id).FirstOrDefaultAsync();

        if (id == null && currentUser == null || id != null && user == null)
            return NotFound();
        if (user!.Role == Role.Student)
            return View("StudentsWall", user);
        var internships = await _repository.GetCollection<Internship>().Find(i => i.UserId == user.Id).ToListAsync();
        ViewData["ownPage"] = true;
        if (currentUser == null || user.Id != currentUser.Id)
        {
            internships = internships.Where(i => i.IsActive).ToList();
            ViewData["ownPage"] = false;
        }

        if (currentUser != null && user.Id != currentUser.Id && currentUser.Role == Role.Student)
            ViewData["student"] = currentUser;

        return View("RepresentativesWall", new RepresentativeWallRepresentation
        {
            User = user, FilterInternshipsRequest = new FilterInternshipsRequest
                { Internships = internships }
        });
    }

    [HttpPost("internships/filter/{ownerId}")]
    public async Task<IActionResult> FilterInternships(string ownerId,
        [FromForm] [Bind("FilterInternshipsRequest")]
        RepresentativeWallRepresentation representation)
    {
        var filterRequest = representation.FilterInternshipsRequest;
        var user = await GetCurrentUser();
        var internships = await _repository.GetCollection<Internship>().Find(i => i.UserId == ownerId).ToListAsync();
        ViewData["ownPage"] = true;
        if (user == null || user.Id != ownerId)
        {
            internships = internships.Where(i => i.IsActive).ToList();
            ViewData["ownPage"] = false;
        }

        if (user != null && user.Id != ownerId && user.Role == Role.Student)
            ViewData["visitor"] = user;
        internships = internships.Where(i =>
        {
            return i.Title.Contains(filterRequest.ComparisonString ?? "") &&
                   i.Topics.Intersect(filterRequest.Topics.Keys.Where(key => filterRequest.Topics[key])).Any();
        }).ToList();
        filterRequest.Internships = internships;
        return PartialView("_FilterInternshipsPartial", filterRequest);
    }

    [AcceptVerbs("GET")]
    [Route("projects/{id}")]
    public async Task<IActionResult> RepresentProject(string id)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return NotFound();
        var proj = await _repository.GetCollection<Project>().Find(proj => proj.Id == id).FirstOrDefaultAsync();
        if (proj == null)
            return NotFound();
        var user = await GetCurrentUser();
        if (!proj.IsPinned && (user == null || user.Id != proj.UserId))
            return Forbid();
        return View(proj);
    }

    [HttpGet("internships/{id}")]
    public async Task<IActionResult> RepresentInternship(string id)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return NotFound();
        var internship = await _repository.GetCollection<Internship>().Find(i => i.Id == id).FirstOrDefaultAsync();
        if (internship == null)
            return NotFound();
        var user = await GetCurrentUser();
        if (!internship.IsActive && (user == null || user.Id != internship.UserId))
            return Forbid();
        if (user is { Role: Role.Student })
        {
            ViewData["student"] = user;
        }

        return View(internship);
    }

    [HttpPost("internships/newRequest")]
    [Authorize(Policy = "OnlyForStudents")]
    public async Task<IActionResult> CreateInternshipRequest(InternshipRequest request)
    {
        var collection = _repository.GetCollection<Internship>();
        var filter = Builders<Internship>.Filter.Eq(i => i.Id, request.InternshipId);
        var internship = await collection.Find(i => i.Id == request.InternshipId).FirstOrDefaultAsync();
        var requests = internship.InternshipRequests.ToList();
        if (requests.Select(r => r.StudentId).Contains(request.StudentId))
            return Forbid();
        requests.Add(request);
        var update = Builders<Internship>.Update.Set(i => i.InternshipRequests, requests.ToArray());
        await collection.UpdateOneAsync(filter, update);
        return RedirectToAction("Wall", new { id = internship.UserId });
    }

    [HttpGet("students")]
    public async Task<IActionResult> AllStudents()
    {
        var usersCollection = _repository.GetCollection<User>();
        var students = await usersCollection.Find(u => u.Role == Role.Student).ToListAsync();
        var projectsCollection = _repository.GetCollection<Project>();
        var projects = await projectsCollection.Find(proj => true).ToListAsync();
        var studentProjects = students.Select(st => new Student
        {
            User = st,
            ProjectCount = projects.Count(proj => proj.IsPinned && proj.UserId == st.Id)
        });
        return View(new FilterStudentsRequest { Students = studentProjects.ToList() });
    }

    [HttpPost("students/filter")]
    public async Task<IActionResult> FilterStudents([Bind("ComparisonString")] FilterStudentsRequest filterRequest)
    {
        var collection = _repository.GetCollection<User>();
        var substr = filterRequest.ComparisonString ?? "";
        var filter = Builders<User>.Filter.Where(user => user.Role == Role.Student &&
                                                         (string.IsNullOrEmpty(substr) || 
                                                          user.PersonalData.Name.ToLower().Contains(substr) ||
                                                          user.PersonalData.Surname.ToLower().Contains(substr) ||
                                                          user.PersonalData.About.ToLower().Contains(substr) ||
                                                          user.PersonalData.Education.ToLower().Contains(substr) ||
                                                          user.PersonalData.Career.ToLower().Contains(substr) ||
                                                          user.PersonalData.City.ToLower().Contains(substr)));
        var students = await collection.Find(filter).ToListAsync();
        var projectsCollection = _repository.GetCollection<Project>();
        var projects = await projectsCollection.Find(proj => true).ToListAsync();
        var studentProjects = students.Select(st => new Student
        {
            User = st,
            ProjectCount = projects.Count(proj => proj.IsPinned && proj.UserId == st.Id)
        });
        return PartialView("_FilterStudentsPartial", new FilterStudentsRequest { Students = studentProjects.ToList() });
    }

    [NonAction]
    private bool CheckUser(User user, string substring)
    {
        return user.Role == Role.Student && (string.IsNullOrEmpty(substring) || (user.PersonalData.Name?.Contains(substring) ?? false) ||
                                             (user.PersonalData.Surname?.Contains(substring) ?? false) ||
                                             (user.PersonalData.About?.Contains(substring) ?? false) ||
                                             (user.PersonalData.Education?.Contains(substring) ?? false) ||
                                             (user.PersonalData.Career?.Contains(substring) ?? false) ||
                                             (user.PersonalData.City?.Contains(substring) ?? false));
    }

    [NonAction]
    private async Task<User?> GetCurrentUser()
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
}