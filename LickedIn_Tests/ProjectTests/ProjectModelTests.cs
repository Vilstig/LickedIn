using System.ComponentModel.DataAnnotations;
using LickedIn.Models;

namespace LickedIn_Tests.ProjectTests
{
    public class ProjectModelTests
    {
        [Fact]
        public void Validate_EndDateBeforeStartDate_ReturnsError() // sprawdzamy czy nie mozemy stworzyć projektu z datą zakończenia przed datą rozpoczęcia
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                ManagerId = 1,
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = new DateOnly(2024, 1, 9)
            };

            var validationContext = new ValidationContext(project);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(project, validationContext, validationResults, true);
            
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(Project.EndDate)));
            Assert.Contains(validationResults, r => r.ErrorMessage == "Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.");
        }

        [Fact]
        public void Validate_EndDateAfterStartDate_ReturnsSuccess() // sprawdzamy czy mozemy stworzyć projekt z datą zakończenia po dacie rozpoczęcia
        {
            var project = new Project
            {
                Name = "Test Project",
                ManagerId = 1,
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = new DateOnly(2024, 1, 20)
            };

            var validationContext = new ValidationContext(project);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(project, validationContext, validationResults, true);

            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Validate_EndDateNull_ReturnsSuccess() // sprawdzamy czy możemy stworzyć projekt bez daty zakończenia
        {
            var project = new Project
            {
                Name = "Test Project",
                ManagerId = 1,
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = null // Dozwolone
            };

            var validationContext = new ValidationContext(project);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(project, validationContext, validationResults, true);

            Assert.True(isValid);
            Assert.Empty(validationResults);
        }
    }
}
