using Microsoft.EntityFrameworkCore;
using LickedIn.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LickedIn.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<SkillType> SkillTypes { get; set; }
        public DbSet<Competency> Competencies { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<VacancySkill> VacancySkills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Competency>()
                .HasIndex(c => new { c.EmployeeId, c.SkillTypeId })
                .IsUnique();

            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.EmployeeId })
                .IsUnique();
        }
    }
}