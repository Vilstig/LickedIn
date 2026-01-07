using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    public class SkillType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<Competency> Competencies { get; set; } = [];
        public SkillType()
        {
            Name = string.Empty;
        }

        public SkillType(string name)
        {
            Name = name;
        }
    }
}