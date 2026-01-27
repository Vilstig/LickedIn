using LickedIn.Data;
using LickedIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LickedIn.Controllers
{
    /// <summary>
    /// Kontroler odpowiedzialny za realizację Przypadku Użycia: "Wystaw oceny członkom zespołu".
    /// Obsługuje dashboard oceniania, formularze ocen oraz anonimizację danych.
    /// </summary>
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyświetla listę zakończonych projektów, które wymagają oceny członków zespołu.
        /// </summary>
        /// <returns>Widok z listą projektów.</returns>
        public async Task<IActionResult> Dashboard()
        {
            var closedProjects = await _context.Projects
                .Include(p => p.Assignments)
                .ThenInclude(pa => pa.Ratings)
                .Where(p => p.EndDate != null)
                .ToListAsync();

            var projectsToRate = closedProjects
                .Where(p => p.Assignments.Any() && p.Assignments.Any(a => !a.Ratings.Any()))
                .ToList();

            return View(projectsToRate);
        }

        /// <summary>
        /// Wyświetla listę członków zespołu danego projektu w celu wyboru osoby do oceny.
        /// </summary>
        /// <param name="projectId">Identyfikator projektu.</param>
        public async Task<IActionResult> ProjectMembers(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Assignments)
                .ThenInclude(pa => pa.Employee)
                .Include(p => p.Assignments)
                .ThenInclude(pa => pa.Ratings)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.EndDate == null)
            {
                TempData["Error"] = "Projekt nie jest zakończony lub nie istnieje.";
                return RedirectToAction(nameof(Dashboard));
            }

            return View(project);
        }

        /// <summary>
        /// Wyświetla formularz oceny dla konkretnego przydziału projektowego.
        /// </summary>
        /// <param name="assignmentId">Identyfikator przydziału (kogo oceniamy).</param>
        public async Task<IActionResult> Rate(int assignmentId)
        {
            var existingRating = await _context.MonthlyRatings
                .FirstOrDefaultAsync(r => r.ProjectAssignmentId == assignmentId);

            if (existingRating != null)
            {
                TempData["Error"] = "Ten pracownik został już oceniony w tym projekcie.";
                var pa = await _context.ProjectAssignments.FindAsync(assignmentId);
                return RedirectToAction(nameof(ProjectMembers), new { projectId = pa.ProjectId });
            }

            var assignment = await _context.ProjectAssignments
                .Include(pa => pa.Employee)
                .Include(pa => pa.Project)
                .FirstOrDefaultAsync(pa => pa.Id == assignmentId);

            if (assignment == null) return NotFound();

            ViewBag.EmployeeName = $"{assignment.Employee.FirstName} {assignment.Employee.LastName}";
            ViewBag.ProjectName = assignment.Project.Name;

            var model = new MonthlyRating
            {
                ProjectAssignmentId = assignmentId,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Score = 5 
            };

            return View(model);
        }

        /// <summary>
        /// Przetwarza wystawioną ocenę. Wykonuje anonimizację komentarza i sprawdza potrzebę rozwoju.
        /// </summary>
        /// <param name="rating">Obiekt oceny.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate([Bind("ProjectAssignmentId,Score,Comment,Date")] MonthlyRating rating)
        {
            if (await _context.MonthlyRatings.AnyAsync(r => r.ProjectAssignmentId == rating.ProjectAssignmentId))
            {
                 var pa = await _context.ProjectAssignments.FindAsync(rating.ProjectAssignmentId);
                 return RedirectToAction(nameof(ProjectMembers), new { projectId = pa.ProjectId });
            }

            if (ModelState.IsValid)
            {
                rating.Comment = AnonymizeComment(rating.Comment);

                _context.Add(rating);
                await _context.SaveChangesAsync();

                if (rating.Score < 3)
                {
                    TempData["DevelopmentProposal"] = "Ocena jest niska. Wygenerowano propozycję szkolenia dla pracownika.";
                }

                var assignment = await _context.ProjectAssignments
                    .FirstOrDefaultAsync(pa => pa.Id == rating.ProjectAssignmentId);

                return RedirectToAction(nameof(ProjectMembers), new { projectId = assignment.ProjectId });
            }
            return View(rating);
        }

        /// <summary>
        /// Funkcja pomocnicza usuwająca dane wrażliwe z komentarza (np. słowa "Kierownik").
        /// </summary>
        /// <param name="comment">Oryginalny komentarz.</param>
        /// <returns>Zanonimizowany ciąg znaków.</returns>
        private string AnonymizeComment(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return "Brak komentarza";
            return comment.Replace("Kierownik", "Anonim").Replace("Szef", "Anonim");
        }
    }
}