using Microsoft.EntityFrameworkCore;
using LickedIn.Models;

namespace LickedIn.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<SkillType> SkillTypes { get; set; }
        public DbSet<Competency> Competencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Competency>()
                .HasIndex(c => new { c.EmployeeId, c.SkillTypeId })
                .IsUnique();
        }
    }
}