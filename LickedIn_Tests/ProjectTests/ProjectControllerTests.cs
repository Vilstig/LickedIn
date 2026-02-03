using LickedIn.Controllers;
using LickedIn.Data;
using LickedIn.Models;
using LickedIn.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LickedIn_Tests.ProjectTests
{
    public class ProjectControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task Create_WithVacancies_AssignsBestCandidate() // sprawdzamy czy faktycznie przypisuje najlepszego pracownika na daną pozycję
        {
            using var context = GetInMemoryDbContext();

            var csharp = new SkillType { Id = 1, Name = "C#" };
            context.SkillTypes.Add(csharp);
            
            var junior = new Employee { Id = 101, FirstName = "Jan", LastName = "Junior" };
            context.Employees.Add(junior);
            context.Competencies.Add(new Competency { EmployeeId = 101, SkillTypeId = 1, Level = 2 });

            var senior = new Employee { Id = 102, FirstName = "Adam", LastName = "Senior" };
            context.Employees.Add(senior);
            context.Competencies.Add(new Competency { EmployeeId = 102, SkillTypeId = 1, Level = 8 });

            var manager = new Employee { Id = 99, FirstName = "Boss", LastName = "Man" };
            context.Employees.Add(manager);

            await context.SaveChangesAsync();

            var controller = new ProjectController(context);

            var model = new ProjectCreateViewModel
            {
                Name = "Test Project",
                ManagerId = 99,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TeamMembers = new List<ProjectMemberRequirement>
                {
                    new ProjectMemberRequirement
                    {
                        RoleName = "Backend Dev",
                        RequiredSkills = new List<VacancySkillRequirement>
                        {
                            new VacancySkillRequirement { SkillTypeId = 1, Level = 5 }
                        }
                    }
                }
            };

            var result = await controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);

            var project = await context.Projects.Include(p => p.ProjectMembers).FirstOrDefaultAsync(p => p.Name == "Test Project");
            Assert.NotNull(project);
            Assert.Single(project.ProjectMembers);

            var member = project.ProjectMembers.First();
            Assert.Equal(senior.Id, member.EmployeeId);
        }

        [Fact]
        public async Task Create_WhenCandidateOccupied_DoesNotAssignTwice() // sprawdzamy czy nie przypisuje jednego pracownika dwukrotnie
        {
            using var context = GetInMemoryDbContext();

            var java = new SkillType { Id = 1, Name = "Java" };
            context.SkillTypes.Add(java);

            var dev = new Employee { Id = 101, FirstName = "One", LastName = "Dev" };
            context.Employees.Add(dev);
            context.Competencies.Add(new Competency { EmployeeId = 101, SkillTypeId = 1, Level = 5 });
            
            var manager = new Employee { Id = 99, FirstName = "Boss", LastName = "Man" };
            context.Employees.Add(manager);
            
            await context.SaveChangesAsync();

            var controller = new ProjectController(context);

            var model = new ProjectCreateViewModel
            {
                Name = "Project Valid",
                ManagerId = 99,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TeamMembers = new List<ProjectMemberRequirement>
                {
                    new ProjectMemberRequirement 
                    { 
                        RequiredSkills = new List<VacancySkillRequirement> { new VacancySkillRequirement { SkillTypeId = 1, Level = 1 } } 
                    },
                    new ProjectMemberRequirement 
                    { 
                        RequiredSkills = new List<VacancySkillRequirement> { new VacancySkillRequirement { SkillTypeId = 1, Level = 1 } } 
                    }
                }
            };

            await controller.Create(model);

            var project = await context.Projects.Include(p => p.ProjectMembers).FirstOrDefaultAsync();
            Assert.NotNull(project);
            
            Assert.Equal(2, project.ProjectMembers.Count);
            var occupied = project.ProjectMembers.Count(pm => pm.EmployeeId == dev.Id);
            var empty = project.ProjectMembers.Count(pm => pm.EmployeeId == null);

            Assert.Equal(1, occupied);
            Assert.Equal(1, empty); 
        }
    }
}
