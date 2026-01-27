using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LickedIn.Controllers
{
    /// <summary>
    /// Kontroler odpowiedzialny za realizację Przypadku Użycia: "Zarządzaj rolami w projekcie".
    /// Obsługuje przypisywanie, edycję i usuwanie ról oraz walidację kompetencji.
    /// </summary>
    public class ProjectRoleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectRoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyświetla panel zarządzania zespołem dla danego projektu.
        /// </summary>
        /// <param name="projectId">Identyfikator projektu.</param>
        /// <returns>Widok z listą członków zespołu.</returns>
        public async Task<IActionResult> Manage(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Assignments)
                .ThenInclude(pa => pa.Employee)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.Name;
            ViewBag.IsProjectClosed = project.EndDate != null; 
            
            return View(project.Assignments);
        }

        /// <summary>
        /// Wyświetla formularz przypisywania nowej roli pracownikowi.
        /// </summary>
        /// <param name="projectId">Identyfikator projektu.</param>
        /// <returns>Formularz przypisania roli lub przekierowanie, jeśli projekt jest zamknięty.</returns>
        public async Task<IActionResult> Assign(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            if (project.EndDate != null) return RedirectToAction(nameof(Manage), new { projectId });

            var assignedEmployeeIds = await _context.ProjectAssignments
                .Where(pa => pa.ProjectId == projectId && pa.EndDate == null)
                .Select(pa => pa.EmployeeId)
                .ToListAsync();

            var availableEmployees = await _context.Employees
                .Where(e => !assignedEmployeeIds.Contains(e.Id))
                .ToListAsync();

            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = projectId;
            ViewData["EmployeeId"] = new SelectList(availableEmployees, "Id", "LastName");
            
            var roles = new List<string> { "Lider Techniczny", "Programista", "Tester", "Analityk" };
            ViewData["Role"] = new SelectList(roles);

            return View();
        }

        /// <summary>
        /// Przetwarza żądanie przypisania roli. Uruchamia walidację kompetencji.
        /// </summary>
        /// <param name="assignment">Obiekt przydziału z formularza.</param>
        /// <returns>Przekierowanie do listy lub powrót do formularza w przypadku błędu walidacji.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign([Bind("ProjectId,EmployeeId,Role,StartDate")] ProjectAssignment assignment)
        {
            var project = await _context.Projects.FindAsync(assignment.ProjectId);
            if (project != null && project.EndDate != null) 
            {
                 return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
            }

            if (ModelState.IsValid)
            {
                if (!await ValidateCompetence(assignment.EmployeeId, assignment.Role))
                {
                    ModelState.AddModelError("", $"Pracownik nie posiada kompetencji dla roli: {assignment.Role}.");
                }
                else
                {
                    _context.Add(assignment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
                }
            }

            ViewBag.ProjectName = project?.Name;
            ViewBag.ProjectId = assignment.ProjectId;
            
            var employees = await _context.Employees.ToListAsync(); 
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "LastName", assignment.EmployeeId);
            
            var roles = new List<string> { "Lider Techniczny", "Programista", "Tester", "Analityk" };
            ViewData["Role"] = new SelectList(roles, assignment.Role);

            return View(assignment);
        }

        /// <summary>
        /// Wyświetla formularz edycji istniejącej roli pracownika.
        /// </summary>
        /// <param name="id">Identyfikator przydziału.</param>
        public async Task<IActionResult> EditRole(int id)
        {
            var assignment = await _context.ProjectAssignments
                .Include(pa => pa.Project)
                .Include(pa => pa.Employee)
                .FirstOrDefaultAsync(pa => pa.Id == id);

            if (assignment == null) return NotFound();

            if (assignment.Project.EndDate != null)
            {
                return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
            }

            ViewBag.EmployeeName = $"{assignment.Employee.FirstName} {assignment.Employee.LastName}";
            ViewBag.ProjectName = assignment.Project.Name;

            var roles = new List<string> { "Lider Techniczny", "Programista", "Tester", "Analityk" };
            ViewData["Role"] = new SelectList(roles, assignment.Role);

            return View(assignment);
        }

        /// <summary>
        /// Zapisuje zmienioną rolę po uprzedniej walidacji kompetencji.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(int id, [Bind("Id,ProjectId,EmployeeId,Role,StartDate")] ProjectAssignment assignment)
        {
            if (id != assignment.Id) return NotFound();

            var project = await _context.Projects.FindAsync(assignment.ProjectId);
            
            if (project != null && project.EndDate != null)
            {
                 return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
            }

            if (ModelState.IsValid)
            {
                if (!await ValidateCompetence(assignment.EmployeeId, assignment.Role))
                {
                    ModelState.AddModelError("", $"Pracownik nie posiada kompetencji dla nowej roli: {assignment.Role}.");
                }
                else
                {
                    try
                    {
                        _context.Update(assignment);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.ProjectAssignments.Any(pa => pa.Id == assignment.Id)) return NotFound();
                        else throw;
                    }
                    return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
                }
            }

            var emp = await _context.Employees.FindAsync(assignment.EmployeeId);
            ViewBag.EmployeeName = $"{emp?.FirstName} {emp?.LastName}";
            ViewBag.ProjectName = project?.Name;

            var roles = new List<string> { "Lider Techniczny", "Programista", "Tester", "Analityk" };
            ViewData["Role"] = new SelectList(roles, assignment.Role);

            return View(assignment);
        }

        /// <summary>
        /// Usuwa przydział (rolę) z projektu.
        /// </summary>
        /// <param name="id">Identyfikator przydziału do usunięcia.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var assignment = await _context.ProjectAssignments
                .Include(pa => pa.Project)
                .FirstOrDefaultAsync(pa => pa.Id == id);

            if (assignment != null)
            {
                if (assignment.Project.EndDate != null)
                {
                    return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
                }

                _context.ProjectAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage), new { projectId = assignment.ProjectId });
            }
            return NotFound();
        }

        /// <summary>
        /// Logika biznesowa sprawdzająca zgodność kompetencji pracownika z wymaganiami roli.
        /// </summary>
        /// <param name="employeeId">Identyfikator pracownika.</param>
        /// <param name="role">Nazwa roli, do której pracownik aspiruje.</param>
        /// <returns>True, jeśli pracownik spełnia wymagania; False w przeciwnym razie.</returns>
        private async Task<bool> ValidateCompetence(int employeeId, string role)
        {
            var employeeCompetencies = await _context.Competencies
                .Where(c => c.EmployeeId == employeeId)
                .ToListAsync();

            if (role == "Lider Techniczny")
            {
                return employeeCompetencies.Any(c => c.Level >= 6);
            }
            
            return employeeCompetencies.Any(c => c.Level >= 1);
        }
    }
}