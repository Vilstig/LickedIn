using Microsoft.Playwright;
using Xunit;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class FjTests
{
    private const string BaseUrl = "http://localhost:5176";
    private async Task LoginAsAdminAsync(IPage page)
    {
        await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("admin@localhost");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Admin123!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
    }

    private async Task CreateEmployeeAsync(IPage page, string lastName, string pesel, string firstName = "Jan", string birthDate = "2000-01-01", string phone = "123456789", string email = "testowy@localhost.pl")
    {
        await page.GetByRole(AriaRole.Link, new() { Name = " Pracownicy" }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = " Dodaj pracownika" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Imię" }).FillAsync(firstName);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Nazwisko" }).FillAsync(lastName);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "PESEL" }).FillAsync(pesel);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Data urodzenia" }).FillAsync(birthDate);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Telefon" }).FillAsync(phone);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync(email);
        await page.GetByRole(AriaRole.Button, new() { Name = "Dodaj pracownika" }).ClickAsync();
    }

    private async Task DeleteEmployeeAsync(IPage page, string uniqueLastName)
    {
        await page.GotoAsync($"{BaseUrl}/Employee");

        var employeeRow = page.Locator("tr").Filter(new() { HasText = uniqueLastName });

        if (await employeeRow.CountAsync() > 0)
        {
            await employeeRow.GetByRole(AriaRole.Link, new() { Name = "Usuń" }).ClickAsync();
            
            await page.GetByRole(AriaRole.Button, new() { Name = "Tak, usuń" }).ClickAsync();

            await Assertions.Expect(employeeRow).ToHaveCountAsync(0);
        }
    }

    [Fact]
    public async Task Test_1_CreateEmployee_SuccessPath()
    {
        var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 200 });
        var page = await browser.NewPageAsync();

        await LoginAsAdminAsync(page);

        string uniquePesel = DateTime.Now.Ticks.ToString().Substring(7, 11);
        string uniqueLastName = "Tester_" + DateTime.Now.Ticks;

        try
        {
            await CreateEmployeeAsync(page, uniqueLastName, uniquePesel);

        
            await Assertions.Expect(page).ToHaveURLAsync($"{BaseUrl}/Employee");
            await Assertions.Expect(page.GetByText(uniquePesel).First).ToBeVisibleAsync();
        }
        finally
        {
            await DeleteEmployeeAsync(page, uniqueLastName);
        }
        
    }

    [Fact]
    public async Task Test_5_AddCompetency_SuccessPath()
    {
        var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 200 });
        var page = await browser.NewPageAsync();
        
        await LoginAsAdminAsync(page);

        string uniquePesel = DateTime.Now.Ticks.ToString().Substring(7, 11);
        string uniqueLastName = "Tester_" + DateTime.Now.Ticks;
        try
        {
          await CreateEmployeeAsync(page, uniqueLastName, uniquePesel);

            var employeeRow = page.Locator("tr").Filter(new() { HasText = uniqueLastName });
            await employeeRow.GetByRole(AriaRole.Link, new() { Name = "Szczegóły" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Przypisz umiejętność" }).ClickAsync();
            await page.GetByLabel("Wybierz umiejętność").SelectOptionAsync(new[] { "3" });
            await page.GetByRole(AriaRole.Spinbutton, new() { Name = "Poziom (1-10)" }).ClickAsync();
            await page.GetByRole(AriaRole.Spinbutton, new() { Name = "Poziom (1-10)" }).FillAsync("4");
            await page.GetByRole(AriaRole.Button, new() { Name = "Przypisz" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/Employee/Details/.*"));

            var competencyRow = page.Locator("tr").Filter(new() { HasText = "Angielski" });

            await Assertions.Expect(competencyRow).ToBeVisibleAsync();

            await Assertions.Expect(competencyRow).ToContainTextAsync("4");  
        }
        finally
        {
            await DeleteEmployeeAsync(page, uniqueLastName);
        }
    }
}