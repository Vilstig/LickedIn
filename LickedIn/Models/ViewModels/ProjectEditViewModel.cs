using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models.ViewModels
{
    public class ProjectEditViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        public string Name { get; set; }

        [Required]
        public int ManagerId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        // Lista wakatów (istniejących i nowych)
        public List<ProjectMemberEditDto> TeamMembers { get; set; } = new List<ProjectMemberEditDto>();

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

    public class ProjectMemberEditDto
    {
        public int Id { get; set; } // 0 = nowy wakat, >0 = istniejący

        // Informacyjnie - kto tu pracuje (tylko do odczytu)
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        // Umiejętności do edycji
        public List<VacancySkillRequirement> RequiredSkills { get; set; } = new List<VacancySkillRequirement>();
    }
}