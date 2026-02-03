# LickedIn Technical Documentation

## 1. Project Overview
**LickedIn** is an Employee and Project Management System designed to optimize the allocation of human resources to projects based on skill requirements. The system features an automated matching algorithm that assigns the most suitable employees to project vacancies.

### Technology Stack
-   **Framework**: ASP.NET Core MVC (.NET 9)
-   **Database**: SQLite (Development) using Entity Framework Core
-   **Authentication/Authorization**: ASP.NET Core Identity
-   **Front-end**: Razor Views usually combined with Bootstrap

## 2. Architecture
The application follows the **Model-View-Controller (MVC)** architectural pattern.
-   **Models**: Define the data structure and business entities.
-   **Views**: Responsible for the presentation layer (UI).
-   **Controllers**: Handle user input, process requests, interact with the database, and select Views to render.

## 3. Data Access Layer
The application uses **Entity Framework Core** for data access. The `ApplicationDbContext` class manages the entity sets and database configuration.

### Database Context: `ApplicationDbContext`
Inherits from `IdentityDbContext` to support authentication.
-   **DbSets**:
    -   `Employees`: Stores employee details.
    -   `SkillTypes`: Dictionary of available skills (e.g., C#, Management).
    -   `Competencies`: Junction table linking `Employees` and `SkillTypes` with a proficiency `Level` (1-10).
    -   `Projects`: Main project entities.
    -   `ProjectMembers`: Represents lines in a project team (vacancies or assigned roles).
    -   `VacancySkills`: Requirements for a specific `ProjectMember` slot.

### Key Entities
#### `Employee`
Represents a staff member.
-   Key relationships: One-to-Many with `Competency`.

#### `Project`
Represents a project.
-   **Attributes**: `Name`, `StartDate`, `EndDate`.
-   **Relationships**: 
    -   `Manager` (Foreign Key to `Employee`).
    -   One-to-Many with `ProjectMember`.
-   **Validation**: Implements `IValidatableObject` to ensure `EndDate` is not earlier than `StartDate`.

#### `ProjectMember` (Vacancy)
Represents a specific role or slot within a project.
-   **State**: 
    -   **Assigned**: `EmployeeId` is set.
    -   **Vacancy**: `EmployeeId` is `null`.
-   **Relationships**: One-to-Many with `VacancySkill`.

#### `VacancySkill`
Defines the technical requirements for a project member slot.
-   Links a `ProjectMember` to a `SkillType` with a required `Level`.

## 4. Business Logic & Controllers

### 4.1 Project Controller (`ProjectController.cs`)
This controller contains the core business logic for project management and team assembly.

#### Key Actions:
-   **Create (POST)**: 
    -   Creates a new project.
    -   Iterates through requested team members.
    -   **Auto-Assignment**: Immediately attempts to find the best candidate for each requested slot using the Matching Algorithm (see Section 5).
    -   Saves the project, members, and skill requirements in a database transaction.
-   **Edit (POST)**:
    -   Updates project details.
    -   Handles adding new vacancies or removing existing ones.
    -   **Validation**: Prevents deletion of a vacancy if it is currently assigned to an employee.
-   **FillVacancies (POST)**:
    -   Triggered manually from the Project Details view.
    -   Identifies all empty slots (`EmployeeId` is null).
    -   Runs the Matching Algorithm to populate these slots with available candidates.
    -   Uses transactions to ensure atomicity.

### 4.2 Employee Controller (`EmployeeController.cs`)
Manages the CRUD operations for employees.
-   Allows HR to add new employees and view their profiles.

### 4.3 Competency Controller (`CompetencyController.cs`)
Manages the skills assigned to employees.
-   Allows defining the proficiency level (1-10) for each skill an employee possesses.

## 5. Automated Matching Algorithm
The system employs a "Lowest Skill Deficit" algorithm to assign candidates to vacancies.

### Logic Flow:
1.  **Scope**: The algorithm considers all employees *except* the Project Manager and those already assigned to the current project.
2.  **Deficit Calculation**:
    For each candidate and each vacancy:
    -   Iterate through the `VacancySkills` (requirements).
    -   Compare the candidate's actual skill level against the required level.
    -   **Formula**: `Deficit = RequiredLevel - min(ActualLevel, RequiredLevel)`
    -   This ensures that exceeding requirements does not provide "bonus points" (deficit floor is 0), but missing requirements adds to the deficit.
3.  **Selection**: 
    -   The candidate with the **lowest total deficit** across all required skills is selected.
    -   If a match is found, the candidate is assigned (`ProjectMember.EmployeeId` is updated).
    -   The candidate is then marked as "assigned" within the scope of the current operation to prevent double-booking.

## 6. Security & Configuration
### Authentication
-   Uses **ASP.NET Core Identity**.
-   Default configuration requirements:
    -   Password length: 3 characters (Development settings).
    -   No requirements for digits, uppercase, or non-alphanumeric characters.

### Authorization
-   **Policies**:
    -   `"HR"` Policy: Requires the user to have the "HR" role.
    -   Applied to `ProjectController` to restrict project management access.
-   **Data Seeding**:
    -   `MyIdentityDataInitializer` seeds initial users and roles.

### Database
-   Configured in `Program.cs` to use SQLite.
-   Connection string: `DefaultConnection` from `appsettings.json`.
