using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class SaatSistemiGuncellemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalismaSaatleri",
                table: "SporSalonlari");

            migrationBuilder.DropColumn(
                name: "MusaitlikSaatleri",
                table: "Antrenorler");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AcilisSaati",
                table: "SporSalonlari",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "KapanisSaati",
                table: "SporSalonlari",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MesaiBaslangic",
                table: "Antrenorler",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MesaiBitis",
                table: "Antrenorler",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcilisSaati",
                table: "SporSalonlari");

            migrationBuilder.DropColumn(
                name: "KapanisSaati",
                table: "SporSalonlari");

            migrationBuilder.DropColumn(
                name: "MesaiBaslangic",
                table: "Antrenorler");

            migrationBuilder.DropColumn(
                name: "MesaiBitis",
                table: "Antrenorler");

            migrationBuilder.AddColumn<string>(
                name: "CalismaSaatleri",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MusaitlikSaatleri",
                table: "Antrenorler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
