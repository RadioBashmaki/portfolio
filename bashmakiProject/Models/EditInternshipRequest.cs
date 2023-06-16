using System.ComponentModel.DataAnnotations;

namespace bashmakiProject.Models;

public class EditInternshipRequest
{
    [Required(ErrorMessage = "Это обязательное поле")]
    public string Title { get; set; }
    public Dictionary<Topic, bool> Topics { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    public Experience Experience { get; set; }
    [Required(ErrorMessage = "Это обязательное поле")]
    [StringLength(5000)]
    public string Description { get; set; }

    public bool IsActive { get; set; }

    public EditProjectFileDescription[]? FilesDescriptions { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();
        if (Topics.Values.All(x => x == false))
            errors.Add(new ValidationResult("Выберите хотя бы одну тематику", new List<string> { "Topics" }));
        return errors;
    }
}