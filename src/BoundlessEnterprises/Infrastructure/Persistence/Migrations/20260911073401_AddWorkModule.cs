using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoundlessEnterprises.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "work");

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssigneeName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deliverable = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dependencies = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Blockers = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    NextStep = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReviewRequired = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    References = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rings",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Disciplines = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HolderName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    HolderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeroTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HeroSubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Focus = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Motto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccentColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HeroImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodePrefix = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    NextSequence = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resources = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentUpdates",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentUpdates_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "work",
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Code",
                schema: "work",
                table: "Assignments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_RingId",
                schema: "work",
                table: "Assignments",
                column: "RingId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentUpdates_AssignmentId",
                schema: "work",
                table: "AssignmentUpdates",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Rings_Domain",
                schema: "work",
                table: "Rings",
                column: "Domain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentUpdates",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Rings",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Assignments",
                schema: "work");
        }
    }
}
