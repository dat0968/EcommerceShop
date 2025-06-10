using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class refactorComboAndCTCommboTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaCombo",
                table: "COMBO");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CHITIETCOMBO");

            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "CHITIETCOMBO");

            migrationBuilder.DropColumn(
                name: "NgayKetThuc",
                table: "CHITIETCOMBO");

            migrationBuilder.DropColumn(
                name: "PhanTramGiam",
                table: "CHITIETCOMBO");

            migrationBuilder.DropColumn(
                name: "SoLuong",
                table: "CHITIETCOMBO");

            migrationBuilder.RenameColumn(
                name: "SoTienGiam",
                table: "CHITIETCOMBO",
                newName: "SoLuongSP");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDau",
                table: "COMBO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKetThuc",
                table: "COMBO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PhanTramGiam",
                table: "COMBO",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienGiam",
                table: "COMBO",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "COMBO");

            migrationBuilder.DropColumn(
                name: "NgayKetThuc",
                table: "COMBO");

            migrationBuilder.DropColumn(
                name: "PhanTramGiam",
                table: "COMBO");

            migrationBuilder.DropColumn(
                name: "SoTienGiam",
                table: "COMBO");

            migrationBuilder.RenameColumn(
                name: "SoLuongSP",
                table: "CHITIETCOMBO",
                newName: "SoTienGiam");

            migrationBuilder.AddColumn<int>(
                name: "GiaCombo",
                table: "COMBO",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CHITIETCOMBO",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDau",
                table: "CHITIETCOMBO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKetThuc",
                table: "CHITIETCOMBO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PhanTramGiam",
                table: "CHITIETCOMBO",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SoLuong",
                table: "CHITIETCOMBO",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
