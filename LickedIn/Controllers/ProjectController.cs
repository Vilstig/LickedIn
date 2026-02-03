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
                .Include(p => p.ProjectMembers)
                .AsNoTracking()
                .ToListAsync();
            return View(projects);
        }

        // GET: Project/Create (Ekran tworzenia projektu)
        public IActionResult Create()
        {
            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName");
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
                    var project = new Project
                    {
                        Name = model.Name,
                        ManagerId = model.ManagerId,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate
                    };
                    _context.Add(project);
                    await _context.SaveChangesAsync();

                    var candidates = await _context.Employees
                        .Include(e => e.Competencies)
                        .Where(e => e.Id != model.ManagerId)
                        .ToListAsync();

                    var assignedEmployeeIds = new HashSet<int>();

                    if (model.TeamMembers != null)
                    {
                        foreach (var memberReq in model.TeamMembers)
                        {
                            var availableCandidates = candidates
                                .Where(c => !assignedEmployeeIds.Contains(c.Id))
                                .ToList();

                            Employee? bestMatch = null;
                            double lowestDeficit = double.MaxValue;

                            if (availableCandidates.Any())
                            {
                                foreach (var candidate in availableCandidates)
                                {
                                    double currentCandidateDeficit = 0;

                                    if (memberReq.RequiredSkills != null)
                                    {
                                        foreach (var skillReq in memberReq.RequiredSkills)
                                        {
                                            var empSkill = candidate.Competencies
                                                .FirstOrDefault(c => c.SkillTypeId == skillReq.SkillTypeId);

                                            int actualLevel = empSkill?.Level ?? 0;
                                            int requiredLevel = skillReq.Level;

                                            double skillDeficit = requiredLevel - Math.Min(actualLevel, requiredLevel);
                                            currentCandidateDeficit += skillDeficit;
                                        }
                                    }

                                    if (currentCandidateDeficit < lowestDeficit)
                                    {
                                        lowestDeficit = currentCandidateDeficit;
                                        bestMatch = candidate;
                                    }
                                }
                            }

                            var member = new ProjectMember
                            {
                                ProjectId = project.Id,
                                EmployeeId = bestMatch?.Id
                            };

                            if (bestMatch != null)
                            {
                                assignedEmployeeIds.Add(bestMatch.Id);
                            }

                            _context.Add(member);
                            await _context.SaveChangesAsync();

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
                    .ThenInclude(pm => pm.Employee)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.RequiredSkills) 
                        .ThenInclude(vs => vs.SkillType) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.RequiredSkills)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            var model = new ProjectEditViewModel
            {
                Id = project.Id,
                Name = project.Name,
                ManagerId = project.ManagerId,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                TeamMembers = project.ProjectMembers.Select(pm => new ProjectMemberEditDto
                {
                    Id = pm.Id,
                    EmployeeId = pm.EmployeeId,
                    EmployeeName = pm.Employee != null ? $"{pm.Employee.FirstName} {pm.Employee.LastName}" : null,
                    RequiredSkills = pm.RequiredSkills.Select(s => new VacancySkillRequirement
                    {
                        SkillTypeId = s.SkillTypeId,
                        Level = s.Level
                    }).ToList()
                }).ToList()
            };

            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", project.ManagerId);
            ViewData["Skills"] = _context.SkillTypes.ToList(); 
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    var projectDb = await _context.Projects
                        .Include(p => p.ProjectMembers)
                            .ThenInclude(pm => pm.Employee)
                        .Include(p => p.ProjectMembers)
                            .ThenInclude(pm => pm.RequiredSkills)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (projectDb == null) return NotFound();

                    projectDb.Name = model.Name;
                    projectDb.ManagerId = model.ManagerId;
                    projectDb.StartDate = model.StartDate;
                    projectDb.EndDate = model.EndDate;

                    var submittedIds = model.TeamMembers.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                    var membersToDelete = projectDb.ProjectMembers.Where(pm => !submittedIds.Contains(pm.Id)).ToList();

                    foreach (var memberToDelete in membersToDelete)
                    {
                        if (memberToDelete.EmployeeId != null)
                        {
                            ModelState.AddModelError("", 
                                $"Nie można usunąć wakatu, który jest wypełniany przez pracownika: {memberToDelete.Employee?.FirstName} {memberToDelete.Employee?.LastName}. Najpierw zwolnij pracownika z projektu.");
                            
                            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", model.ManagerId);
                            ViewData["Skills"] = _context.SkillTypes.ToList();
                            return View(model);
                        }
                        
                        _context.ProjectMembers.Remove(memberToDelete);
                    }

                    foreach (var memberDto in model.TeamMembers)
                    {
                        if (memberDto.Id > 0)
                        {
                            var existingMember = projectDb.ProjectMembers.FirstOrDefault(pm => pm.Id == memberDto.Id);
                            if (existingMember != null)
                            {
                                _context.VacancySkills.RemoveRange(existingMember.RequiredSkills);
                                
                                foreach (var skillDto in memberDto.RequiredSkills)
                                {
                                    _context.VacancySkills.Add(new VacancySkill
                                    {
                                        ProjectMemberId = existingMember.Id,
                                        SkillTypeId = skillDto.SkillTypeId,
                                        Level = skillDto.Level
                                    });
                                }
                            }
                        }
                        else
                        {
                            var newMember = new ProjectMember
                            {
                                ProjectId = projectDb.Id,
                                EmployeeId = null,
                            };
                            _context.ProjectMembers.Add(newMember);
                            await _context.SaveChangesAsync(); 

                            foreach (var skillDto in memberDto.RequiredSkills)
                            {
                                _context.VacancySkills.Add(new VacancySkill
                                {
                                    ProjectMemberId = newMember.Id,
                                    SkillTypeId = skillDto.SkillTypeId,
                                    Level = skillDto.Level
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Błąd zapisu: " + ex.Message);
                }
            }

            ViewData["ManagerId"] = new SelectList(_context.Employees, "Id", "LastName", model.ManagerId);
            ViewData["Skills"] = _context.SkillTypes.ToList();
            return View(model);
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

            member.EmployeeId = null;
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = member.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FillVacancies(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var emptySlots = await _context.ProjectMembers
                    .Include(pm => pm.RequiredSkills)
                    .Where(pm => pm.ProjectId == projectId && pm.EmployeeId == null)
                    .ToListAsync();

                if (!emptySlots.Any())
                {
                    return RedirectToAction(nameof(Details), new { id = projectId });
                }

                var existingTeamIds = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == projectId && pm.EmployeeId != null)
                    .Select(pm => pm.EmployeeId.Value)
                    .ToListAsync();

                var assignedIds = new HashSet<int>(existingTeamIds);

                var candidates = await _context.Employees
                    .Include(e => e.Competencies)
                    .Where(e => e.Id != project.ManagerId)
                    .ToListAsync();

                foreach (var slot in emptySlots)
                {
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

                            foreach (var req in slot.RequiredSkills)
                            {
                                var empSkill = candidate.Competencies
                                    .FirstOrDefault(c => c.SkillTypeId == req.SkillTypeId);

                                int actualLevel = empSkill?.Level ?? 0;
                                int requiredLevel = req.Level;

                                currentDeficit += requiredLevel - Math.Min(actualLevel, requiredLevel);
                            }

                            if (currentDeficit < lowestDeficit)
                            {
                                lowestDeficit = currentDeficit;
                                bestMatch = candidate;
                            }
                        }
                    }

                    if (bestMatch != null)
                    {
                        slot.EmployeeId = bestMatch.Id;
                        assignedIds.Add(bestMatch.Id);
                        _context.Update(slot);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}