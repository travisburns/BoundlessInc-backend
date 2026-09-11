using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoundlessEnterprises.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "intelligence");

            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.CreateTable(
                name: "BusinessEvents",
                schema: "intelligence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailySnapshots",
                schema: "intelligence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EventCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Integrations",
                schema: "integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiKeyPrefix = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastEventAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEvents_CompanyId",
                schema: "intelligence",
                table: "BusinessEvents",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEvents_CompanyId_EventType",
                schema: "intelligence",
                table: "BusinessEvents",
                columns: new[] { "CompanyId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEvents_OccurredAtUtc",
                schema: "intelligence",
                table: "BusinessEvents",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DailySnapshots_CompanyId_Date",
                schema: "intelligence",
                table: "DailySnapshots",
                columns: new[] { "CompanyId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_ApiKeyHash",
                schema: "integration",
                table: "Integrations",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_CompanyId",
                schema: "integration",
                table: "Integrations",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessEvents",
                schema: "intelligence");

            migrationBuilder.DropTable(
                name: "DailySnapshots",
                schema: "intelligence");

            migrationBuilder.DropTable(
                name: "Integrations",
                schema: "integration");
        }
    }
}
