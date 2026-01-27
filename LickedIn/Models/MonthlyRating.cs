using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    /// <summary>
    /// Reprezentuje ocenę zaangażowania pracownika wystawianą przez kierownika.
    /// Dotyczy konkretnego przydziału projektowego.
    /// </summary>
    public class MonthlyRating
    {
        /// <summary>
        /// Unikalny identyfikator oceny.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Klucz obcy do przydziału projektowego, którego dotyczy ocena.
        /// </summary>
        [Required]
        public int ProjectAssignmentId { get; set; }
        
        /// <summary>
        /// Obiekt nawigacyjny przydziału.
        /// </summary>
        public ProjectAssignment? ProjectAssignment { get; set; }

        /// <summary>
        /// Wartość liczbowa oceny (skala 1-10).
        /// </summary>
        [Required]
        [Range(1, 10)]
        public int Score { get; set; }

        /// <summary>
        /// Opcjonalny komentarz słowny do oceny.
        /// </summary>
        [StringLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Data wystawienia oceny.
        /// </summary>
        public DateOnly Date { get; set; }
    }
}