using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LickedIn.Models
{
    // Odzwierciedla tabelę 'wakat', która łączy projekt z pracownikiem
    public class ProjectMember
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public ICollection<VacancySkill> RequiredSkills { get; set; } = new List<VacancySkill>();
    }
}