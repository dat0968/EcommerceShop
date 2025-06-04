using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class Add_DanhGia_ToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DANHGIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShopPhanHoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaSp = table.Column<int>(type: "int", nullable: true),
                    MaCombo = table.Column<int>(type: "int", nullable: true),
                    MaKh = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DANHGIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DANHGIA_COMBO_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "COMBO",
                        principalColumn: "MaCombo");
                    table.ForeignKey(
                        name: "FK_DANHGIA_KHACHHANG_MaKh",
                        column: x => x.MaKh,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DANHGIA_SANPHAM_MaSp",
                        column: x => x.MaSp,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaCombo",
                table: "DANHGIA",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaKh_MaCombo",
                table: "DANHGIA",
                columns: new[] { "MaKh", "MaCombo" },
                unique: true,
                filter: "[MaCombo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaKh_MaSp",
                table: "DANHGIA",
                columns: new[] { "MaKh", "MaSp" },
                unique: true,
                filter: "[MaSp] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaSp",
                table: "DANHGIA",
                column: "MaSp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DANHGIA");
        }
    }
}
