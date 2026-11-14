using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CqrsEsTodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyEventSourceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacySource",
                table: "TodoEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacySource",
                table: "TodoEvents");
        }
    }
}
