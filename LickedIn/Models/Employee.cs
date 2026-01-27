using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(11, MinimumLength = 11)] 
        public string Pesel { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [StringLength(100)]
        [EmailAddress] 
        public string? Email { get; set; } 

        public ICollection<Competency> Competencies { get; set; } = [];

        public ICollection<ProjectAssignment> ProjectAssignments { get; set; } = [];

        public Employee()
        {
        }

        public Employee(string firstName, string lastName, string pesel, DateOnly dateOfBirth, string phoneNumber, string? email = null)
        {
            FirstName = firstName;
            LastName = lastName;
            Pesel = pesel;
            DateOfBirth = dateOfBirth;
            PhoneNumber = phoneNumber;
            Email = email;
        }
    }
}