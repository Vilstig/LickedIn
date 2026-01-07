using System.ComponentModel.DataAnnotations;

namespace LickedIn.Models
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(11, MinimumLength = 11)] 
        public string Pesel { get; set; }

        public DateOnly DateOfBirth { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [StringLength(100)]
        [EmailAddress] 
        public string? Email { get; set; } 

        public ICollection<Competency> Competencies { get; set; } = [];
        public Employee()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Pesel = string.Empty;
            PhoneNumber = string.Empty;
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