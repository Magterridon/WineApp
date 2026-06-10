using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WineApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Ingredients = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    RecipeType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    DrinkFromYear = table.Column<int>(type: "integer", nullable: true),
                    DrinkToYear = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cellars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cellars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cellars_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeWinePairings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    WineId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeWinePairings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeWinePairings_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeWinePairings_Wines_WineId",
                        column: x => x.WineId,
                        principalTable: "Wines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WineCepages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WineId = table.Column<int>(type: "integer", nullable: false),
                    CepageName = table.Column<string>(type: "text", nullable: false),
                    Percentage = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WineCepages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WineCepages_Wines_WineId",
                        column: x => x.WineId,
                        principalTable: "Wines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CellarItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CellarId = table.Column<int>(type: "integer", nullable: false),
                    WineId = table.Column<int>(type: "integer", nullable: false),
                    BottleCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellarItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellarItems_Cellars_CellarId",
                        column: x => x.CellarId,
                        principalTable: "Cellars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CellarItems_Wines_WineId",
                        column: x => x.WineId,
                        principalTable: "Wines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CellarItems_CellarId_WineId",
                table: "CellarItems",
                columns: new[] { "CellarId", "WineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CellarItems_WineId",
                table: "CellarItems",
                column: "WineId");

            migrationBuilder.CreateIndex(
                name: "IX_Cellars_UserId",
                table: "Cellars",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeWinePairings_RecipeId",
                table: "RecipeWinePairings",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeWinePairings_WineId",
                table: "RecipeWinePairings",
                column: "WineId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WineCepages_WineId",
                table: "WineCepages",
                column: "WineId");

            migrationBuilder.CreateIndex(
                name: "IX_Wines_Name_Domain_Year",
                table: "Wines",
                columns: new[] { "Name", "Domain", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CellarItems");

            migrationBuilder.DropTable(
                name: "RecipeWinePairings");

            migrationBuilder.DropTable(
                name: "WineCepages");

            migrationBuilder.DropTable(
                name: "Cellars");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Wines");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
