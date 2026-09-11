using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoundlessEnterprises.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingStepKinds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "onboarding",
                table: "Steps",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "onboarding",
                table: "EmployeeSteps",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponseJson",
                schema: "onboarding",
                table: "EmployeeSteps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "onboarding",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "onboarding",
                table: "EmployeeSteps");

            migrationBuilder.DropColumn(
                name: "ResponseJson",
                schema: "onboarding",
                table: "EmployeeSteps");
        }
    }
}
