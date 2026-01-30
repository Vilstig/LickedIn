using LickedIn.Controllers;
using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CompetencyControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Create_Post_ReturnsError_WhenCompetencyAlreadyExists()
    {
        var context = GetDbContext();

        context.Competencies.Add(new Competency { EmployeeId = 1, SkillTypeId = 10, Level = 1 });
        await context.SaveChangesAsync();

        var controller = new CompetencyController(context);
        var newCompetency = new Competency { EmployeeId = 1, SkillTypeId = 10, Level = 2 };

        var result = await controller.Create(newCompetency);

        Assert.False(controller.ModelState.IsValid);
        var error = controller.ModelState[""]?.Errors[0].ErrorMessage;
        Assert.Equal("Ten pracownik już posiada tę kompetencję.", error);
    }

    [Fact]
    public async Task Create_Post_RedirectsAndSaves_WhenCompetencyIsNew()
    {
        var context = GetDbContext();
        var controller = new CompetencyController(context);
        
        var newCompetency = new Competency 
        { 
            EmployeeId = 1, 
            SkillTypeId = 20, 
            Level = 3 
        };

        var result = await controller.Create(newCompetency);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Employee", redirectResult.ControllerName);

        var savedCompetency = await context.Competencies
            .FirstOrDefaultAsync(c => c.EmployeeId == 1 && c.SkillTypeId == 20);
        
        Assert.NotNull(savedCompetency);
        Assert.Equal(3, savedCompetency.Level);
    }
}