using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models.ViewModels
{
    public class ProjectCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Nazwa jest wymagana")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Wybierz kierownika")]
        public int ManagerId { get; set; }
        
        [Required]
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public List<ProjectMemberRequirement> TeamMembers { get; set; } = new List<ProjectMemberRequirement>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.HasValue && EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.", 
                    new[] { nameof(EndDate) }
                );
            }
        }
    }

    public class ProjectMemberRequirement
    {
        public string RoleName { get; set; } 

        public List<VacancySkillRequirement> RequiredSkills { get; set; } = new List<VacancySkillRequirement>();
    }

    public class VacancySkillRequirement
    {
        public int SkillTypeId { get; set; }
        
        [Range(1, 10)]
        public int Level { get; set; }
    }
}