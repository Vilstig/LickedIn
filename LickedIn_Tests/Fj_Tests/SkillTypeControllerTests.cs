using LickedIn.Controllers;
using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SkillTypeControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikalna baza dla każdego testu
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Create_Post_ReturnsError_WhenSkillTypeAlreadyExists()
    {
        var context = GetDbContext();
        context.SkillTypes.Add(new SkillType { Name = "C#" });
        await context.SaveChangesAsync();

        var controller = new SkillTypeController(context);
        var newSkillType = new SkillType { Name = "c#" };

        var result = await controller.Create(newSkillType);

        Assert.False(controller.ModelState.IsValid);
        var error = controller.ModelState["Name"]?.Errors[0].ErrorMessage;
        Assert.Equal("Taka umiejętność już istnieje w bazie.", error);
    }

    [Fact]
    public async Task Delete_ReturnsErro_WhenSkillTypeInUse()
    {
        var context = GetDbContext();
        var skillType = new SkillType { Name = "Java" };
        context.SkillTypes.Add(skillType);
        context.Competencies.Add(new Competency { EmployeeId = 1, SkillType = skillType, Level = 2 });
        await context.SaveChangesAsync();

        var controller = new SkillTypeController(context);
        var result = await controller.Delete(skillType.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var errorMessage = viewResult.ViewData["DeleteError"] as string;
        Assert.Contains("Nie można usunąć umiejętności", errorMessage);
        Assert.Contains("Java", errorMessage);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetDbContext();
        var controller = new SkillTypeController(context);

        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_DoesNotReturnError_WhenSkillHasNoCompetencies()
    {
        var context = GetDbContext();
        var skillWithoutCompetency = new SkillType { Id = 2, Name = "Python" };
        context.SkillTypes.Add(skillWithoutCompetency);
        await context.SaveChangesAsync();

        var controller = new SkillTypeController(context);

        var result = await controller.Delete(skillWithoutCompetency.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewData["DeleteError"]);
    }
}