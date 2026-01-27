using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models.ViewModels
{
    public class ProjectCreateViewModel
    {
        // --- Dane Projektu ---
        [Required(ErrorMessage = "Nazwa jest wymagana")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Wybierz kierownika")]
        public int ManagerId { get; set; }
        
        [Required]
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        // --- Lista Wakatów (Członków Zespołu) ---
        public List<ProjectMemberRequirement> TeamMembers { get; set; } = new List<ProjectMemberRequirement>();
    }

    public class ProjectMemberRequirement
    {
        // Np. "Backend Developer", "Tester" - pomocnicze pole do opisu roli
        public string RoleName { get; set; } 

        // Lista umiejętności wymagana od TEJ KONKRETNEJ osoby
        public List<VacancySkillRequirement> RequiredSkills { get; set; } = new List<VacancySkillRequirement>();
    }

    public class VacancySkillRequirement
    {
        public int SkillTypeId { get; set; }
        
        [Range(1, 10)]
        public int Level { get; set; }
    }
}