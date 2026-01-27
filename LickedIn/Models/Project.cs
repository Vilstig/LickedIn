using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty; 

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public ICollection<ProjectAssignment> Assignments { get; set; } = [];
    }
}