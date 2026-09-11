using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoundlessEnterprises.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRingFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RingFiles",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RingFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RingFiles_AssignmentId",
                schema: "work",
                table: "RingFiles",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RingFiles_RingId",
                schema: "work",
                table: "RingFiles",
                column: "RingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RingFiles",
                schema: "work");
        }
    }
}
