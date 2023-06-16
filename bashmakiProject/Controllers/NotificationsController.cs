using System.Security.Claims;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace bashmakiProject.Controllers;

[Route("[controller]")]
public class NotificationsController : Controller
{
    private readonly IMongoDbRepository _repository;

    public NotificationsController(IMongoDbRepository repo)
    {
        _repository = repo;
    }
    
    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> Notifications()
    {
        var user = await GetCurrentUser();
        var notificationsCollection = _repository.GetCollection<Notification>();
        var notifications = await notificationsCollection.Find(n => n.ToWhomId == user!.Id).ToListAsync();
        return PartialView("_NotificationsPartial", notifications);
    }
    
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateNotification(Notification notification)
    {
        var notificationsCollection = _repository.GetCollection<Notification>();
        
        notification.CreationDate = DateTime.Now.ToLocalTime();

        await notificationsCollection.InsertOneAsync(notification);
        
        return Json(true);
    }

    [HttpPost("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        var collection = _repository.GetCollection<Notification>();
        await collection.DeleteOneAsync(n => n.Id == id);
        return Json(true);
    }
    
    [NonAction]
    private async Task<User?> GetCurrentUser()
    {
        var collection = _repository.GetCollection<User>();
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var filter = Builders<User>.Filter
            .Eq(u => u.Id, id);
        var user = await collection
            .Find(filter)
            .FirstOrDefaultAsync();
        return user;
    }
}