using System.ComponentModel.DataAnnotations;
using LickedIn.Models.ViewModels;

namespace LickedIn_Tests.ProjectTests
{
    public class ProjectCreateViewModelTests
    {
        [Fact]
        public void Validate_EndDateBeforeStartDate_ShouldReturnError()
        {
            var model = new ProjectCreateViewModel
            {
                Name = "Test Project",
                ManagerId = 1,
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = new DateOnly(2024, 1, 9)
            };

            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            Assert.False(isValid, "ViewModel should be invalid when EndDate < StartDate");
        }
    }
}
