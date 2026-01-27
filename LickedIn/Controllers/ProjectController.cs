using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LickedIn.Models.ViewModels;

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
            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName");
            // Potrzebne do dynamicznego dodawania wierszy w JS
            ViewData["Skills"] = _context.SkillTypes.ToList(); 
            return View(new ProjectCreateViewModel { StartDate = DateOnly.FromDateTime(DateTime.Now) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    // 1. Zapis Projektu
                    var project = new Project
                    {
                        Name = model.Name,
                        ManagerId = model.ManagerId,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate
                    };
                    _context.Add(project);
                    await _context.SaveChangesAsync();

                    // 2. Pobranie wszystkich kandydatów z ich kompetencjami
                    var candidates = await _context.Employees
                        .Include(e => e.Competencies)
                        .Where(e => e.Id != model.ManagerId)
                        .ToListAsync();

                    // Zbiór ID pracowników już przypisanych do tego projektu (żeby nie sklonować osoby)
                    var assignedEmployeeIds = new HashSet<int>();

                    // 3. Iteracja po zdefiniowanych wakatach (rolach)
                    if (model.TeamMembers != null)
                    {
                        foreach (var memberReq in model.TeamMembers)
                        {
                            // Filtrujemy dostępnych kandydatów
                            var availableCandidates = candidates
                                .Where(c => !assignedEmployeeIds.Contains(c.Id))
                                .ToList();

                            Employee? bestMatch = null;
                            double lowestDeficit = double.MaxValue;

                            // Szukamy najlepszego kandydata dla TEGO konkretnego zestawu umiejętności
                            if (availableCandidates.Any())
                            {
                                foreach (var candidate in availableCandidates)
                                {
                                    double currentCandidateDeficit = 0;

                                    // Sumujemy braki we wszystkich wymaganych umiejętnościach dla tej roli
                                    if (memberReq.RequiredSkills != null)
                                    {
                                        foreach (var skillReq in memberReq.RequiredSkills)
                                        {
                                            var empSkill = candidate.Competencies
                                                .FirstOrDefault(c => c.SkillTypeId == skillReq.SkillTypeId);

                                            int actualLevel = empSkill?.Level ?? 0;
                                            int requiredLevel = skillReq.Level;

                                            // Deficyt = ile brakuje do ideału
                                            double skillDeficit = requiredLevel - Math.Min(actualLevel, requiredLevel);
                                            currentCandidateDeficit += skillDeficit;
                                        }
                                    }

                                    // Jeśli ten kandydat jest lepszy (mniejszy deficyt) niż poprzedni najlepszy
                                    // W przypadku remisu (np. obaj mają 0 deficytu) - wygrywa pierwszy w kolejności (można dodać losowość)
                                    if (currentCandidateDeficit < lowestDeficit)
                                    {
                                        lowestDeficit = currentCandidateDeficit;
                                        bestMatch = candidate;
                                    }
                                }
                            }

                            // Jeśli znaleziono kandydata (nawet słabego), przypisujemy go
                            if (bestMatch != null)
                            {
                                assignedEmployeeIds.Add(bestMatch.Id); // Zablokuj go

                                var member = new ProjectMember
                                {
                                    ProjectId = project.Id,
                                    EmployeeId = bestMatch.Id
                                };
                                _context.Add(member);
                                await _context.SaveChangesAsync(); // Zapisz, by mieć ID

                                // Zapisujemy wymagania wakatu (historia, co było potrzebne na to stanowisko)
                                if (memberReq.RequiredSkills != null)
                                {
                                    foreach (var skillReq in memberReq.RequiredSkills)
                                    {
                                        _context.Add(new VacancySkill
                                        {
                                            ProjectMemberId = member.Id,
                                            SkillTypeId = skillReq.SkillTypeId,
                                            Level = skillReq.Level
                                        });
                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Details), new { id = project.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Błąd: " + ex.Message);
                }
            }

            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", model.ManagerId);
            ViewData["Skills"] = _context.SkillTypes.ToList();
            return View(model);
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Employee) // Ładowanie pracowników
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.RequiredSkills) // <--- TO BYŁO BRAKUJĄCE
                        .ThenInclude(vs => vs.SkillType)  // <--- I TO (żeby widzieć nazwę skilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int id)
        {
            var member = await _context.ProjectMembers.FindAsync(id);
            if (member == null) return NotFound();

            // Zamiast usuwać rekord, czyścimy przypisanie pracownika
            // Dzięki temu zachowujemy wymagania (VacancySkills) dla tego stanowiska
            member.EmployeeId = null;
            
            await _context.SaveChangesAsync();
            
            // Wracamy do widoku szczegółów projektu
            return RedirectToAction(nameof(Details), new { id = member.ProjectId });
        }

        // AKCJA 2: Automatyczne uzupełnianie wakatów
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FillVacancies(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. Pobierz wszystkie PUSTE wakaty w tym projekcie (wraz z ich wymaganiami)
                var emptySlots = await _context.ProjectMembers
                    .Include(pm => pm.RequiredSkills)
                    .Where(pm => pm.ProjectId == projectId && pm.EmployeeId == null)
                    .ToListAsync();

                if (!emptySlots.Any())
                {
                    return RedirectToAction(nameof(Details), new { id = projectId });
                }

                // 2. Pobierz ID pracowników już przypisanych do tego projektu (żeby ich nie dublować)
                var existingTeamIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == projectId && pm.EmployeeId != null)
                    .Select(pm => pm.EmployeeId.Value)
                    .ToListAsync();

                // HashSet dla szybkiego sprawdzania i blokowania nowo dobranych w tej transakcji
                var assignedIds = new HashSet<int>(existingTeamIds);

                // 3. Pobierz dostępnych kandydatów (z pominięciem managera)
                var candidates = await _context.Employees
                    .Include(e => e.Competencies)
                    .Where(e => e.Id != project.ManagerId)
                    .ToListAsync();

                // 4. Algorytm doboru dla każdego pustego miejsca
                foreach (var slot in emptySlots)
                {
                    // Filtrujemy kandydatów: tylko ci, którzy nie są w tym projekcie
                    var availableCandidates = candidates
                        .Where(c => !assignedIds.Contains(c.Id))
                        .ToList();

                    Employee? bestMatch = null;
                    double lowestDeficit = double.MaxValue;

                    if (availableCandidates.Any())
                    {
                        foreach (var candidate in availableCandidates)
                        {
                            double currentDeficit = 0;

                            // Obliczamy dopasowanie do wymagań TEGO KONKRETNEGO wakatu
                            foreach (var req in slot.RequiredSkills)
                            {
                                var empSkill = candidate.Competencies
                                    .FirstOrDefault(c => c.SkillTypeId == req.SkillTypeId);

                                int actualLevel = empSkill?.Level ?? 0;
                                int requiredLevel = req.Level;

                                // Wzór deficytu
                                currentDeficit += requiredLevel - Math.Min(actualLevel, requiredLevel);
                            }

                            if (currentDeficit < lowestDeficit)
                            {
                                lowestDeficit = currentDeficit;
                                bestMatch = candidate;
                            }
                        }
                    }

                    // Przypisanie znalezionego kandydata
                    if (bestMatch != null)
                    {
                        slot.EmployeeId = bestMatch.Id;
                        assignedIds.Add(bestMatch.Id); // Zablokuj go dla kolejnych iteracji pętli
                        _context.Update(slot);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                // Opcjonalnie: Obsługa błędu (TempData)
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}