using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace bashmakiProject.Controllers;

public class AccountController : Controller
{
    private readonly IMongoDbRepository _repository;

    public AccountController(IMongoDbRepository repository)
    {
        _repository = repository;
    }

    [AcceptVerbs("GET", "POST")]
    [Route("auth")]
    [Route("[controller]/auth")]
    public async Task<IActionResult> Auth([FromForm] AuthRequest authRequest, string? returnUrl = null)
    {
        if (Request.Method == "GET")
        {
            ViewData["returnUrl"] = returnUrl;
            foreach (var field in ModelState.Keys)
                ModelState.ClearValidationState(field);
            return View();
        }

        var collection = _repository.GetCollection<User>();
        var user = await collection.Find(u => u.Email == authRequest.Email).SingleOrDefaultAsync();
        if (user == null || !MatchPasswordHash(authRequest.Password, user.Password, user.PasswordKey))
        {
            ModelState.AddModelError("Password", "Неверный e-mail или пароль");
            return View(authRequest);
        }

        await AuthorizeUser(user);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }


    [AcceptVerbs("GET", "POST")]
    [Route("register")]
    [Route("[controller]/register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest registerRequest)
    {
        if (Request.Method == "GET")
        {
            foreach (var field in ModelState.Keys)
                ModelState.ClearValidationState(field);
            return View();
        }

        if (await UserExists(registerRequest.Email))
            ModelState.AddModelError("Email", $"e-mail {registerRequest.Email} уже используется");
        if (!ModelState.IsValid)
            return View(registerRequest);

        byte[] passwordHash, passwordKey;

        using (var hmac = new HMACSHA512())
        {
            passwordKey = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerRequest.Password));
        }

        var user = new User
        {
            Email = registerRequest.Email,
            Password = passwordHash,
            PasswordKey = passwordKey,
            Role = registerRequest.Role,
            PersonalData = new PersonalData
            {
                Name = registerRequest.Name,
                Surname = registerRequest.Surname,
                DateOfBirth = registerRequest.DateOfBirth,
                Gender = registerRequest.Gender,
                Company = registerRequest.Company
            }
        };

        await _repository.GetCollection<User>().InsertOneAsync(user);
        await AuthorizeUser(user);

        return RedirectToAction("Index", "Home");
    }

    [AcceptVerbs("GET", "POST")]
    [Route("[controller]/verifyEmail")]
    public async Task<IActionResult> VerifyEmail(string email)
    {
        return await UserExists(email) ? Json($"e-mail {email} уже используется") : Json(true);
    }

    [NonAction]
    private async Task<bool> UserExists(string email)
    {
        var collection = _repository.GetCollection<User>();
        var user = await collection.Find(u => u.Email == email).FirstOrDefaultAsync();
        return user != null;
    }

    [NonAction]
    private static bool MatchPasswordHash(string passwordString, byte[] passwordBytes, byte[] passwordKey)
    {
        using var hmac = new HMACSHA512(passwordKey);
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordString));

        return !passwordBytes.Where((t, i) => t != passwordHash[i]).Any();
    }

    [NonAction]
    private async Task AuthorizeUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()!)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = false,
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            properties);
    }

    [AcceptVerbs("GET")]
    [Route("logout")]
    [Route("[controller]/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [AcceptVerbs("GET", "POST")]
    [Route("profile")]
    [Route("[controller]/profile")]
    public async Task<IActionResult> Profile([FromForm] PersonalData personalData)
    {
        var collection = _repository.GetCollection<User>();
        var mail = User.FindFirstValue(ClaimTypes.Email);
        var filter = Builders<User>.Filter
            .Eq(u => u.Email, mail);
        var user = await collection
            .Find(filter)
            .FirstOrDefaultAsync();
        
        if (Request.Method == "GET")
            return View(user.PersonalData);

        personalData.Avatar = user.PersonalData.Avatar;
        var avatar = Request.Form.Files.FirstOrDefault();
        if (avatar != null)
        {
            using var ms = new MemoryStream();
            await avatar.OpenReadStream().CopyToAsync(ms);
            personalData.Avatar = ms.ToArray();
        }

        var update = Builders<User>.Update
            .Set(u => u.PersonalData, personalData);
        var res = await collection.UpdateOneAsync(filter, update);
        ViewData["dataSaved"] = res.IsAcknowledged;

        return View(personalData);
    }

    [AcceptVerbs("GET")]
    [Route("[controller]/avatar")]
    public async Task<IActionResult> GetUsersAvatar()
    {
        var user = await GetCurrentUser();
        if (user.PersonalData.Avatar == null)
            return NotFound();
        return File(user.PersonalData.Avatar, "image/png");
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