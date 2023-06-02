using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace bashmakiProject.Models;

public enum Role
{
    Student,
    Representative
}

public enum Gender
{
    Male,
    Female
}

public class RegisterRequest{
    [Required(ErrorMessage = "Это обязательное поле")]
    public Role? Role { get; set; }

    [Required(ErrorMessage = "Это обязательное поле")]
    [EmailAddress(ErrorMessage = "Пожалуйста, введите корректый e-mail")]
    [Remote("VerifyEmail", "Account")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Это обязательное поле")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Длина пароля должна быть от 8 до 20 символов")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Это обязательное поле")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Длина пароля должна быть от 8 до 20 символов")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string PasswordConfirm { get; set; }

    public string? Name { get; set; }
    public string? Surname { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Company { get; set; }
}