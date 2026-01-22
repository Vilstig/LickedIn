using LickedIn.Controllers;
using LickedIn.Data;
using LickedIn.Models;
using Microsoft.EntityFrameworkCore;

public class CompetencyControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikalna baza dla każdego testu
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Create_Post_ReturnsError_WhenCompetencyAlreadyExists()
    {
        // Arrange
        var context = GetDbContext();
        // Dodajemy istniejącą kompetencję do bazy w pamięci
        context.Competencies.Add(new Competency { EmployeeId = 1, SkillTypeId = 10, Level = 1 });
        await context.SaveChangesAsync();

        var controller = new CompetencyController(context);
        var newCompetency = new Competency { EmployeeId = 1, SkillTypeId = 10, Level = 2 };

        // Act
        var result = await controller.Create(newCompetency);

        // Assert
        Assert.False(controller.ModelState.IsValid);
        var error = controller.ModelState[""]?.Errors[0].ErrorMessage;
        Assert.Equal("Ten pracownik już posiada tę kompetencję.", error);
    }
}