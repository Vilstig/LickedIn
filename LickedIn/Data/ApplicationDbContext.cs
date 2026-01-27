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
        
        // NOWE TABELE
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; } // Tabela 'wakat'
        public DbSet<VacancySkill> VacancySkills { get; set; }   // Tabela 'umiejetnosc_wakatu'

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Istniejąca konfiguracja Competency...
            modelBuilder.Entity<Competency>()
                .HasIndex(c => new { c.EmployeeId, c.SkillTypeId })
                .IsUnique();

            // Konfiguracja dla ProjectMember (Wakat)
            // Zakładamy, że jeden pracownik może być w projekcie tylko raz (opcjonalnie)
            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.EmployeeId })
                .IsUnique();
        }
    }
}