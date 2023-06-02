using System.ComponentModel.DataAnnotations;

namespace bashmakiProject.Models;

public class AuthRequest
{
    [Required(ErrorMessage = "Это обязательное поле")]
    [EmailAddress(ErrorMessage = "Пожалуйста, введите корректый e-mail")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Длина пароля должна быть от 8 до 20 символов")]
    public string Password { get; set; }
}