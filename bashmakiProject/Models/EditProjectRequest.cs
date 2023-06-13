using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace bashmakiProject.Models;

public class EditProjectRequest : IValidatableObject
{
    [Required(ErrorMessage = "Это обязательное поле")]
    public string Title { get; set; }

    public Dictionary<Topic, bool> Topics { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    [StringLength(5000)]
    public string Description { get; set; }

    public EditProjectFileDescription[]? FilesDescriptions { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();
        if (Topics.Values.All(x => x == false))
            errors.Add(new ValidationResult("Выберите хотя бы одну тематику", new List<string> { "Topics" }));
        return errors;
    }
}

public class EditProjectFileDescription
{
    public string? DatabaseId { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    public string Description { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    public IFormFile? File { get; set; }
    public string? Extension { get; set; }
}