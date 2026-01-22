using LickedIn.Controllers;
using LickedIn.Data;
using LickedIn.Models;
using Microsoft.EntityFrameworkCore;

public class EmployeeControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikalna baza dla każdego testu
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Create_Post_ReturnsError_WhenEmployeeWithSamePeselExists()
    {
        var context = GetDbContext();
        context.Employees.Add(new Employee { FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", PhoneNumber = "123456789" });
        await context.SaveChangesAsync();

        var controller = new EmployeeController(context);
        var newEmployee = new Employee { FirstName = "Anna", LastName = "Nowak", Pesel = "12345678901", PhoneNumber = "987654321" };

        var result = await controller.Create(newEmployee);

        Assert.False(controller.ModelState.IsValid);
        var error = controller.ModelState["Pesel"]?.Errors[0].ErrorMessage;
        Assert.Equal("Pracownik z tym numerem PESEL już istnieje.", error);
    }

    [Fact]
    public async Task Edit_Post_ReturnsError_WhenEmployeeWithSamePeselExists()
    {
        var context = GetDbContext();
        context.Employees.Add(new Employee { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", PhoneNumber = "123456789" });
        context.Employees.Add(new Employee { Id = 2, FirstName = "Anna", LastName = "Nowak", Pesel = "98765432109", PhoneNumber = "987654321" });
        await context.SaveChangesAsync();

        var controller = new EmployeeController(context);
        var editedEmployee = new Employee { Id = 2, FirstName = "Anna", LastName = "Nowak", Pesel = "12345678901", PhoneNumber = "111222333" };

        var result = await controller.Edit(editedEmployee.Id, editedEmployee);

        Assert.False(controller.ModelState.IsValid);
        var error = controller.ModelState["Pesel"]?.Errors[0].ErrorMessage;
        Assert.Equal("Pracownik z tym numerem PESEL już istnieje.", error);
    }
}