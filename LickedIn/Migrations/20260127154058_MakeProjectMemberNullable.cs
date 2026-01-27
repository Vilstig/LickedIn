using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LickedIn.Migrations
{
    /// <inheritdoc />
    public partial class MakeProjectMemberNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Employees_EmployeeId",
                table: "ProjectMembers");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "ProjectMembers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Employees_EmployeeId",
                table: "ProjectMembers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Employees_EmployeeId",
                table: "ProjectMembers");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "ProjectMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Employees_EmployeeId",
                table: "ProjectMembers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
