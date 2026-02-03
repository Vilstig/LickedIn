using System.ComponentModel.DataAnnotations;
using LickedIn.Models.ViewModels;

namespace LickedIn_Tests.ProjectTests
{
    public class ProjectEditViewModelTests
    {
        [Fact]
        public void Validate_EndDateBeforeStartDate_ShouldReturnError()
        {
            // Arrange
            var model = new ProjectEditViewModel
            {
                Id = 1,
                Name = "Test Project",
                ManagerId = 1,
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = new DateOnly(2024, 1, 9) // This SHOULD fail
            };

            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid, "ViewModel should be invalid when EndDate < StartDate");
        }
    }
}
