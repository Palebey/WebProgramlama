using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class SporSalonuGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Telefon",
                table: "SporSalonlari",
                newName: "Hizmetler");

            migrationBuilder.RenameColumn(
                name: "Adres",
                table: "SporSalonlari",
                newName: "Aciklama");

            migrationBuilder.RenameColumn(
                name: "UzmanlikAlani",
                table: "Antrenorler",
                newName: "Uzmanlik");

            migrationBuilder.RenameColumn(
                name: "CalismaSaatleri",
                table: "Antrenorler",
                newName: "MusaitlikSaatleri");

            migrationBuilder.AddColumn<decimal>(
                name: "UyelikUcreti",
                table: "SporSalonlari",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TarihSaat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UyeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AntrenorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Randevular_Antrenorler_AntrenorId",
                        column: x => x.AntrenorId,
                        principalTable: "Antrenorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Randevular_AspNetUsers_UyeId",
                        column: x => x.UyeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_AntrenorId",
                table: "Randevular",
                column: "AntrenorId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_UyeId",
                table: "Randevular",
                column: "UyeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropColumn(
                name: "UyelikUcreti",
                table: "SporSalonlari");

            migrationBuilder.RenameColumn(
                name: "Hizmetler",
                table: "SporSalonlari",
                newName: "Telefon");

            migrationBuilder.RenameColumn(
                name: "Aciklama",
                table: "SporSalonlari",
                newName: "Adres");

            migrationBuilder.RenameColumn(
                name: "Uzmanlik",
                table: "Antrenorler",
                newName: "UzmanlikAlani");

            migrationBuilder.RenameColumn(
                name: "MusaitlikSaatleri",
                table: "Antrenorler",
                newName: "CalismaSaatleri");
        }
    }
}
