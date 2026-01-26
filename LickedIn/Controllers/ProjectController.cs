using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LickedIn.Controllers
{
    [Authorize(Policy = "HR")]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Project (Lista projektów)
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.ProjectMembers) // Załaduj liczbę członków zespołu
                .AsNoTracking()
                .ToListAsync();
            return View(projects);
        }

        // GET: Project/Create (Ekran tworzenia projektu)
        public IActionResult Create()
        {
            // Lista pracowników do wyboru na Kierownika (Managera)
            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName");
            return View();
        }

        // POST: Project/Create (Logika z diagramu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,StartDate,EndDate,ManagerId")] Project project)
        {
            if (ModelState.IsValid)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    // KROK 1: Zapisanie danych projektu (wg diagramu)
                    _context.Add(project);
                    await _context.SaveChangesAsync();

                    // KROK 2: Prośba o dobranie zespołu / Utworzenie zespołu
                    // Logika tymczasowa: Dobieramy losowych 3 pracowników (innych niż manager)
                    var randomTeam = await _context.Employees
                        .Where(e => e.Id != project.ManagerId)
                        .OrderBy(r => EF.Functions.Random()) // Wymaga SQLite/Postgres. Dla SQL Server: Guid.NewGuid()
                        .Take(3)
                        .ToListAsync();

                    foreach (var emp in randomTeam)
                    {
                        var member = new ProjectMember
                        {
                            ProjectId = project.Id,
                            EmployeeId = emp.Id
                        };
                        _context.Add(member);
                    }

                    // KROK 3: Poinformowanie o pomyślnym utworzeniu (Commit)
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    // KROK ALTERNATYWNY Z DIAGRAMU: Usunięcie wprowadzonych danych
                    // Transaction Rollback dzieje się automatycznie przy błędzie.
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia projektu i zespołu.");
                }
            }

            // Jeśli walidacja nie przeszła
            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", project.ManagerId);
            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", project.ManagerId);
            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartDate,EndDate,ManagerId")] Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", project.ManagerId);
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                // Kaskadowe usuwanie członków jest obsługiwane przez DB, 
                // ale dla pewności w EF Core:
                var members = _context.ProjectMembers.Where(pm => pm.ProjectId == id);
                _context.ProjectMembers.RemoveRange(members);
                
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}