using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wybory.Web.Migrations
{
    /// <inheritdoc />
    public partial class PoczatkowaBaza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Komitety",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nazwa = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Komitety", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Okregi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nazwa = table.Column<string>(type: "TEXT", nullable: false),
                    LiczbaMandatow = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Okregi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KomitetyOkregow",
                columns: table => new
                {
                    KomitetId = table.Column<int>(type: "INTEGER", nullable: false),
                    OkregId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KomitetyOkregow", x => new { x.KomitetId, x.OkregId });
                    table.ForeignKey(
                        name: "FK_KomitetyOkregow_Komitety_KomitetId",
                        column: x => x.KomitetId,
                        principalTable: "Komitety",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KomitetyOkregow_Okregi_OkregId",
                        column: x => x.OkregId,
                        principalTable: "Okregi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wyborcy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Pesel = table.Column<string>(type: "TEXT", nullable: false),
                    Imie = table.Column<string>(type: "TEXT", nullable: false),
                    Nazwisko = table.Column<string>(type: "TEXT", nullable: false),
                    OkregId = table.Column<int>(type: "INTEGER", nullable: false),
                    CzynnePrawoWyborcze = table.Column<bool>(type: "INTEGER", nullable: false),
                    BierneProwoWyborcze = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wyborcy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wyborcy_Okregi_OkregId",
                        column: x => x.OkregId,
                        principalTable: "Okregi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kandydaci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WyborcaId = table.Column<int>(type: "INTEGER", nullable: false),
                    KomitetId = table.Column<int>(type: "INTEGER", nullable: false),
                    OkregId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumerNaLiscie = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kandydaci", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kandydaci_Komitety_KomitetId",
                        column: x => x.KomitetId,
                        principalTable: "Komitety",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kandydaci_Okregi_OkregId",
                        column: x => x.OkregId,
                        principalTable: "Okregi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kandydaci_Wyborcy_WyborcaId",
                        column: x => x.WyborcaId,
                        principalTable: "Wyborcy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Glosy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WyborcaId = table.Column<int>(type: "INTEGER", nullable: false),
                    KandydatId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataOddania = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Glosy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Glosy_Kandydaci_KandydatId",
                        column: x => x.KandydatId,
                        principalTable: "Kandydaci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Glosy_Wyborcy_WyborcaId",
                        column: x => x.WyborcaId,
                        principalTable: "Wyborcy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Glosy_KandydatId",
                table: "Glosy",
                column: "KandydatId");

            migrationBuilder.CreateIndex(
                name: "IX_Glosy_WyborcaId",
                table: "Glosy",
                column: "WyborcaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kandydaci_KomitetId",
                table: "Kandydaci",
                column: "KomitetId");

            migrationBuilder.CreateIndex(
                name: "IX_Kandydaci_OkregId",
                table: "Kandydaci",
                column: "OkregId");

            migrationBuilder.CreateIndex(
                name: "IX_Kandydaci_WyborcaId",
                table: "Kandydaci",
                column: "WyborcaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Komitety_Nazwa",
                table: "Komitety",
                column: "Nazwa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KomitetyOkregow_OkregId",
                table: "KomitetyOkregow",
                column: "OkregId");

            migrationBuilder.CreateIndex(
                name: "IX_Wyborcy_OkregId",
                table: "Wyborcy",
                column: "OkregId");

            migrationBuilder.CreateIndex(
                name: "IX_Wyborcy_Pesel",
                table: "Wyborcy",
                column: "Pesel",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Glosy");

            migrationBuilder.DropTable(
                name: "KomitetyOkregow");

            migrationBuilder.DropTable(
                name: "Kandydaci");

            migrationBuilder.DropTable(
                name: "Komitety");

            migrationBuilder.DropTable(
                name: "Wyborcy");

            migrationBuilder.DropTable(
                name: "Okregi");
        }
    }
}
