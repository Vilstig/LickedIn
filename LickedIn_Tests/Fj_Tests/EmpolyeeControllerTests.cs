using LickedIn.Controllers;
using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class EmployeeControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
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

    [Fact]
    public async Task Create_Post_RedirectsAndSaves_WhenDataIsValid()
    {
        var context = GetDbContext();
        var controller = new EmployeeController(context);
        var newEmployee = new Employee 
        { 
            FirstName = "Marek", 
            LastName = "Zieliński", 
            Pesel = "55050512345", 
            PhoneNumber = "500600700" 
        };

        var result = await controller.Create(newEmployee);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var employeeInDb = await context.Employees.FirstOrDefaultAsync(e => e.Pesel == "55050512345");
        Assert.NotNull(employeeInDb);
        Assert.Equal("Marek", employeeInDb.FirstName);
    }

    [Fact]
    public async Task Edit_Post_RedirectsAndUpdates_WhenDataIsValid()
    {
        var context = GetDbContext();
        var existingEmployee = new Employee { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "11111111111", PhoneNumber = "123" };
        context.Employees.Add(existingEmployee);
        await context.SaveChangesAsync();

        context.Entry(existingEmployee).State = EntityState.Detached;

        var controller = new EmployeeController(context);
        var updatedData = new Employee { Id = 1, FirstName = "Janusz", LastName = "Kowalski", Pesel = "11111111111", PhoneNumber = "999" };

        var result = await controller.Edit(1, updatedData);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var employeeInDb = await context.Employees.FindAsync(1);
        Assert.Equal("Janusz", employeeInDb.FirstName);
        Assert.Equal("999", employeeInDb.PhoneNumber);
    }
}