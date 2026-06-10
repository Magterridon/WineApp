using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDrinkRecordPairingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PairingNote",
                table: "DrinkRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PairingRating",
                table: "DrinkRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "DrinkRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrinkRecords_RecipeId",
                table: "DrinkRecords",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkRecords_Recipes_RecipeId",
                table: "DrinkRecords",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkRecords_Recipes_RecipeId",
                table: "DrinkRecords");

            migrationBuilder.DropIndex(
                name: "IX_DrinkRecords_RecipeId",
                table: "DrinkRecords");

            migrationBuilder.DropColumn(
                name: "PairingNote",
                table: "DrinkRecords");

            migrationBuilder.DropColumn(
                name: "PairingRating",
                table: "DrinkRecords");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "DrinkRecords");
        }
    }
}
