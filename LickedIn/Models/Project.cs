using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LickedIn.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa projektu jest wymagana")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Data startu jest wymagana")]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        // Kierownik projektu (FK)
        [Required]
        public int ManagerId { get; set; }
        
        [ForeignKey("ManagerId")]
        public Employee? Manager { get; set; }

        // Relacja do członków zespołu (Wakatów)
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    }
}