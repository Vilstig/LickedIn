using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    /// <summary>
    /// Reprezentuje przypisanie pracownika do konkretnego projektu wraz z pełnioną rolą.
    /// Jest to tabela łącząca w relacji wiele-do-wielu między Pracownikiem a Projektem.
    /// </summary>
    public class ProjectAssignment
    {
        /// <summary>
        /// Unikalny identyfikator przydziału.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Klucz obcy projektu.
        /// </summary>
        [Required]
        public int ProjectId { get; set; }
        
        /// <summary>
        /// Obiekt nawigacyjny do projektu.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Klucz obcy pracownika.
        /// </summary>
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Obiekt nawigacyjny do pracownika.
        /// </summary>
        public Employee? Employee { get; set; }

        /// <summary>
        /// Nazwa roli pełnionej w projekcie (np. "Lider Techniczny", "Programista").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Data rozpoczęcia pracy w danej roli.
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Data zakończenia pracy w danej roli (null, jeśli nadal aktywna).
        /// </summary>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// Kolekcja ocen miesięcznych wystawionych w ramach tego przydziału.
        /// </summary>
        public ICollection<MonthlyRating> Ratings { get; set; } = [];
    }
}