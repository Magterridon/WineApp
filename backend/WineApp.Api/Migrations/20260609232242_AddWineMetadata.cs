using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWineMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS so this is safe to run even if columns were added manually
            migrationBuilder.Sql("ALTER TABLE \"Wines\" ADD COLUMN IF NOT EXISTS \"Appellation\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Wines\" ADD COLUMN IF NOT EXISTS \"Color\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Wines\" ADD COLUMN IF NOT EXISTS \"Country\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Wines\" ADD COLUMN IF NOT EXISTS \"Region\" text;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Appellation",
                table: "Wines");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Wines");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Wines");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Wines");
        }
    }
}
