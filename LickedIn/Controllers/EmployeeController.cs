using LickedIn.Models;
using LickedIn.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace LickedIn.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .AsNoTracking() 
                .ToListAsync();
                
            return View(employees);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Pesel,DateOfBirth,PhoneNumber,Email")] Employee employee)
        {
            if (await _context.Employees.AnyAsync(e => e.Pesel == employee.Pesel))
            {
                ModelState.AddModelError("Pesel", "Pracownik z tym numerem PESEL już istnieje.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(employee);
        }

        public async Task<IActionResult> Details(int? id)
        {
            return View(await GetEmployeeWithCompetenciesAsync(id));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var employee = await GetEmployeeAsync(id);
            if (employee == null) return NotFound();
            
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Pesel,DateOfBirth,PhoneNumber,Email")] Employee employee)
        {
            if (id != employee.Id) return NotFound();
            
            if (await _context.Employees.AnyAsync(e => e.Pesel == employee.Pesel && e.Id != employee.Id))
            {
                ModelState.AddModelError("Pesel", "Pracownik z tym numerem PESEL już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                        return NotFound();
                    
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var employee = await GetEmployeeAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<Employee?> GetEmployeeAsync(int? id)
        {
            if (id == null) return null;

            return await _context.Employees.FindAsync(id);
        }
        private async Task<Employee?> GetEmployeeWithCompetenciesAsync(int? id)
        {
            if (id == null) return null;

            return await _context.Employees
                .Include(e => e.Competencies)
                .ThenInclude(c => c.SkillType)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }   
    }
}