using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    public class Competency
    {
        public int Id { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public int EmployeeId { get; set; } 
        public Employee? Employee { get; set; } 
        
        [Required]
        public int SkillTypeId { get; set; }
        public SkillType? SkillType { get; set; }
    }
}