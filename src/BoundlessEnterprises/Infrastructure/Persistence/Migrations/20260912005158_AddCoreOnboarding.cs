using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoundlessEnterprises.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoreOnboardingProgress",
                schema: "onboarding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CompletedSignature = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreOnboardingProgress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoreOnboardingStageSignatures",
                schema: "onboarding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    StageTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    TypedName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    SignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreOnboardingStageSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoreOnboardingStageSignatures_CoreOnboardingProgress_ProgressId",
                        column: x => x.ProgressId,
                        principalSchema: "onboarding",
                        principalTable: "CoreOnboardingProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoreOnboardingProgress_UserId",
                schema: "onboarding",
                table: "CoreOnboardingProgress",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoreOnboardingStageSignatures_ProgressId_StageKey",
                schema: "onboarding",
                table: "CoreOnboardingStageSignatures",
                columns: new[] { "ProgressId", "StageKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoreOnboardingStageSignatures",
                schema: "onboarding");

            migrationBuilder.DropTable(
                name: "CoreOnboardingProgress",
                schema: "onboarding");
        }
    }
}
