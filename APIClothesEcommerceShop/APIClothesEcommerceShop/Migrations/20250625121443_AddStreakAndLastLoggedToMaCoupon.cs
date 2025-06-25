using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakAndLastLoggedToMaCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaKhachHang",
                table: "MACOUPON",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "KHACHHANG",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TruyCapLlanCuoi",
                table: "KHACHHANG",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_MACOUPON_MaKhachHang",
                table: "MACOUPON",
                column: "MaKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_MACOUPON_KHACHHANG_MaKhachHang",
                table: "MACOUPON",
                column: "MaKhachHang",
                principalTable: "KHACHHANG",
                principalColumn: "MaKH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MACOUPON_KHACHHANG_MaKhachHang",
                table: "MACOUPON");

            migrationBuilder.DropIndex(
                name: "IX_MACOUPON_MaKhachHang",
                table: "MACOUPON");

            migrationBuilder.DropColumn(
                name: "MaKhachHang",
                table: "MACOUPON");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "KHACHHANG");

            migrationBuilder.DropColumn(
                name: "TruyCapLlanCuoi",
                table: "KHACHHANG");
        }
    }
}
