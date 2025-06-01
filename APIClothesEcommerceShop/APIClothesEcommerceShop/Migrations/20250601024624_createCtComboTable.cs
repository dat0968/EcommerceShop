using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class createCtComboTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHITIETCOMBO",
                columns: table => new
                {
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    MaCombo = table.Column<int>(type: "int", nullable: false),
                    PhanTramGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoTienGiam = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHITIETCOMBO", x => new { x.MaSP, x.MaCombo });
                    table.ForeignKey(
                        name: "FK_CHITIETCOMBO_COMBO_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "COMBO",
                        principalColumn: "MaCombo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CHITIETCOMBO_SANPHAM_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETCOMBO_MaCombo",
                table: "CHITIETCOMBO",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETCOMBO_MaSP",
                table: "CHITIETCOMBO",
                column: "MaSP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHITIETCOMBO");
        }
    }
}
