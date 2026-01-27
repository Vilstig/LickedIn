using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LickedIn.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public ICollection<VacancySkill> RequiredSkills { get; set; } = new List<VacancySkill>();
    }
}