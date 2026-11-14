using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CqrsEsTodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyImportRunsAndUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TodoItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LegacyImportRuns",
                columns: table => new
                {
                    BundleSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommitCount = table.Column<int>(type: "int", nullable: false),
                    SourceEventCount = table.Column<int>(type: "int", nullable: false),
                    ImportedEventCount = table.Column<int>(type: "int", nullable: false),
                    ImportedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyImportRuns", x => x.BundleSha256);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegacyImportRuns");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TodoItems");
        }
    }
}
