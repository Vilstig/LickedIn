using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LickedIn.Controllers
{
    [Authorize(Policy = "HR")]
    public class CompetencyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompetencyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Competency/Create?employeeId=5
        public async Task<IActionResult> Create(int? employeeId)
        {
            if (employeeId == null) return NotFound();

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return NotFound();

            ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
            ViewBag.EmployeeId = employeeId;

            var competency = new Competency 
            { 
                EmployeeId = employeeId.Value,
                Level = 1 
            };
            ViewData["SkillTypeId"] = new SelectList(_context.SkillTypes, "Id", "Name");

            return View(competency);
        }

        // POST: Competency/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,SkillTypeId,Level")] Competency competency)
        {
            bool exists = await _context.Competencies.AnyAsync(
                c => c.EmployeeId == competency.EmployeeId && c.SkillTypeId == competency.SkillTypeId);

            if (exists)
            {
                ModelState.AddModelError("", "Ten pracownik już posiada tę kompetencję.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(competency);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Employee");
            }

            var employee = await _context.Employees.FindAsync(competency.EmployeeId);
            ViewBag.EmployeeName = (employee != null) ? $"{employee.FirstName} {employee.LastName}" : "Pracownik";
            ViewData["SkillTypeId"] = new SelectList(_context.SkillTypes, "Id", "Name", competency.SkillTypeId);

            return View(competency);
        }
        
        // GET: Competency/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var competency = await _context.Competencies
                .Include(c => c.Employee)
                .Include(c => c.SkillType)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (competency == null) return NotFound();

            ViewBag.EmployeeName = $"{competency.Employee.FirstName} {competency.Employee.LastName}";
            ViewBag.SkillName = competency.SkillType.Name;

            return View(competency);
        }

        // POST: Competency/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Level")] Competency competency)
        {
            if (id != competency.Id) return NotFound();

            
            var existingCompetency = await _context.Competencies
                .Include(c => c.Employee) 
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCompetency == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingCompetency.Level = competency.Level;
                    
                    _context.Update(existingCompetency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompetencyExists(competency.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index", "Employee");
            }

            ViewBag.EmployeeName = $"{existingCompetency.Employee.FirstName} {existingCompetency.Employee.LastName}";
            ViewBag.SkillName = existingCompetency.SkillType?.Name; 

            return View(competency);
        }

        // GET: Competency/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var competency = await _context.Competencies
                .Include(c => c.Employee)
                .Include(c => c.SkillType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (competency == null) return NotFound();

            return View(competency);
        }

        // POST: Competency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var competency = await _context.Competencies.FindAsync(id);
            if (competency != null)
            {
                _context.Competencies.Remove(competency);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Employee");
        }

        private bool CompetencyExists(int id)
        {
            return _context.Competencies.Any(e => e.Id == id);
        }
    }
}