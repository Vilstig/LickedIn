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
        public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
        public DbSet<MonthlyRating> MonthlyRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Competency>()
                .HasIndex(c => new { c.EmployeeId, c.SkillTypeId })
                .IsUnique();

            modelBuilder.Entity<ProjectAssignment>()
                .HasOne(pa => pa.Project)
                .WithMany(p => p.Assignments)
                .HasForeignKey(pa => pa.ProjectId);

            modelBuilder.Entity<ProjectAssignment>()
                .HasOne(pa => pa.Employee)
                .WithMany(e => e.ProjectAssignments)
                .HasForeignKey(pa => pa.EmployeeId);
        }
    }
}