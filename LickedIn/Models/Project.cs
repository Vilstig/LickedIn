using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LickedIn.Models
{
    public class Project : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa projektu jest wymagana")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Data startu jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        public DateOnly StartDate { get; set; }

        [Display(Name = "Data zakończenia")]
        public DateOnly? EndDate { get; set; }

        [Required]
        public int ManagerId { get; set; }
        
        [ForeignKey("ManagerId")]
        public Employee? Manager { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

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
}