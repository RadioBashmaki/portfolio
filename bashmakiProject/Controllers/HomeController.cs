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
        var user = id == null
            ? await GetCurrentUser()
            : await _repository.GetCollection<User>().Find(u => u.Id == id).FirstOrDefaultAsync();

        if (user == null)
            return NotFound();
        return user.Role == Role.Student ? View("StudentsWall", user) : View("RepresentativesWall", user);
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
}