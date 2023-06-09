﻿using System.Security.Claims;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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
        var filter = Builders<Internship>.Filter.Eq(i => i.UserId, user.Id);
        ViewData["ownPage"] = true;
        if (currentUser == null || user.Id != currentUser.Id)
        {
            filter &= Builders<Internship>.Filter.Eq(i => i.IsActive, true);
            ViewData["ownPage"] = false;
        }
        
        var internships = await _repository.GetCollection<Internship>().Find(filter).ToListAsync();
        
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
        var filter = Builders<Internship>.Filter.Eq(i => i.UserId, ownerId);
        ViewData["ownPage"] = true;
        if (user == null || user.Id != ownerId)
        {
            filter &= Builders<Internship>.Filter.Eq(i => i.IsActive, true);
            ViewData["ownPage"] = false;
        }

        if (user != null && user.Id != ownerId && user.Role == Role.Student)
            ViewData["visitor"] = user;
        var topics = filterRequest.Topics.Keys.Where(key =>
            filterRequest.Topics[key]);
        filter &= Builders<Internship>.Filter.Where(i => i.Title.Contains(filterRequest.ComparisonString ?? "") &&
                                                         i.Topics.Intersect(topics).Any());
        
        var internships = await _repository.GetCollection<Internship>().Find(filter).ToListAsync();
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
        var update = Builders<Internship>.Update.Push(i => i.InternshipRequests, request);
        await collection.UpdateOneAsync(filter, update);
        return RedirectToAction("Wall", new { id = internship.UserId });
    }

    [HttpPost("internships/editRequest")]
    [Authorize(Policy = "OnlyForRepresentatives")]
    public async Task<IActionResult> EditInternshipRequest([Bind("StudentId", "Status", "InternshipId")]InternshipRequest editRequest)
    {
        var internshipsCollection = _repository.GetCollection<Internship>();
        var filter = Builders<Internship>.Filter.Eq(i => i.Id, editRequest.InternshipId) &
                     Builders<Internship>.Filter.ElemMatch(i => i.InternshipRequests, r => r.StudentId == editRequest.StudentId);
        var update = Builders<Internship>.Update.Set(i => i.InternshipRequests.FirstMatchingElement().Status, editRequest.Status);
        await internshipsCollection.UpdateOneAsync(filter, update);
        return Json(true);
    }
    
    [Authorize]
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

    [Authorize]
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

    [Authorize]
    [HttpGet("internships")]
    public async Task<IActionResult> AllInternships()
    {
        var collection = _repository.GetCollection<Internship>();
        var belongs = bool.TryParse(Request.Query["my"], out _) && User.IsInRole("Student");
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var filter = Builders<Internship>.Filter.Where(i =>
            i.IsActive && (!belongs || i.InternshipRequests.Select(r => r.StudentId).Contains(id)));
        var internships = await collection.Find(filter).ToListAsync();
        return View(new FilterInternshipsRequest { Internships = internships });
    }

    [Authorize]
    [HttpPost("internships/filter")]
    public async Task<IActionResult> FilterAllInternships(
        [Bind("ComparisonString", "Topics", "ExperienceDemanded")]
        FilterInternshipsRequest filterRequest)
    {
        var collection = _repository.GetCollection<Internship>();
        var filter = Builders<Internship>.Filter.Where(i =>
            i.IsActive && (filterRequest.ExperienceDemanded && i.Experience == Experience.Demanded
                           || !filterRequest.ExperienceDemanded && i.Experience == Experience.NotDemanded) &&
            (string.IsNullOrEmpty(filterRequest.ComparisonString) ||
             i.Title.ToLower().Contains(filterRequest.ComparisonString) ||
             i.Description.ToLower().Contains(filterRequest.ComparisonString) ||
             i.CompanyName.ToLower().Contains(filterRequest.ComparisonString)));
        var internships = await collection.Find(filter).ToListAsync();
        return PartialView("_FilterAllInternshipsPartial", new FilterInternshipsRequest { Internships = internships });
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