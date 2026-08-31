using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wybory.Web.Migrations
{
    /// <inheritdoc />
    public partial class DodajUnikalnyNumerNaLiscie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kandydaci_KomitetId",
                table: "Kandydaci");

            migrationBuilder.CreateIndex(
                name: "IX_Kandydaci_KomitetId_OkregId_NumerNaLiscie",
                table: "Kandydaci",
                columns: new[] { "KomitetId", "OkregId", "NumerNaLiscie" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kandydaci_KomitetId_OkregId_NumerNaLiscie",
                table: "Kandydaci");

            migrationBuilder.CreateIndex(
                name: "IX_Kandydaci_KomitetId",
                table: "Kandydaci",
                column: "KomitetId");
        }
    }
}
