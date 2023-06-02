using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bashmakiProject.Controllers;

public class HomeController : Controller
{
    [HttpGet("")]
    [HttpGet("index")]
    public IActionResult Index()
    {
        ViewBag.User = User;
        return View();
    }
    
    [Authorize]
    [HttpGet("hello")]
    public IActionResult Hello()
    {
        return Content("Hello");
    }
}