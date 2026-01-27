using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LickedIn.Models
{
    public class VacancySkill
    {
        public int Id { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public int ProjectMemberId { get; set; }
        [ForeignKey("ProjectMemberId")]
        public ProjectMember? ProjectMember { get; set; }

        [Required]
        public int SkillTypeId { get; set; }
        public SkillType? SkillType { get; set; }
    }
}