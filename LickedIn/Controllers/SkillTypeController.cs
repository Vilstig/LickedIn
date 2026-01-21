using System.ComponentModel;
using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LickedIn.Controllers
{
    [Authorize(Policy = "HR")]
    public class SkillTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SkillTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lista wszystkich dostępnych umiejętności (np. tabela)
        // GET: SkillType
        public async Task<IActionResult> Index()
        {
            return View(await _context.SkillTypes.ToListAsync());
        }

        // 2. Formularz dodawania nowej nazwy (np. "C#")
        // GET: SkillType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SkillType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] SkillType skillType)
        {
            // Sprawdzenie czy taka nazwa już istnieje
            if (await _context.SkillTypes.AnyAsync(s => s.Name.ToLower() == skillType.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Taka umiejętność już istnieje w bazie.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(skillType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skillType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var skillType = await _context.SkillTypes
                .Include(s => s.Competencies)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skillType == null) return NotFound();

            if (skillType.Competencies.Any())
            {
               ViewBag.DeleteError = $"Nie można usunąć umiejętności '{skillType.Name}', ponieważ jest przypisana do kompetencji pracowników.";
            }
            return View(skillType);
        } 

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillType = await _context.SkillTypes.FindAsync(id);
            
            if (skillType == null) return RedirectToAction(nameof(Index));

            bool isUsed = await _context.Competencies.AnyAsync(c => c.SkillTypeId == id);
            
            if (isUsed)
            {
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.SkillTypes.Remove(skillType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}