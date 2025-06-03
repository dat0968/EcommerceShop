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
                    MaKh = table.Column<int>(type: "int", nullable: false),
                    MaHd = table.Column<int>(type: "int", nullable: false),
                    MaCtsp = table.Column<int>(type: "int", nullable: true),
                    MaCombo = table.Column<int>(type: "int", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShopPhanHoi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DANHGIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DANHGIA_CHITIETSANPHAM_MaCtsp",
                        column: x => x.MaCtsp,
                        principalTable: "CHITIETSANPHAM",
                        principalColumn: "MaCTSP");
                    table.ForeignKey(
                        name: "FK_DANHGIA_COMBO_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "COMBO",
                        principalColumn: "MaCombo");
                    table.ForeignKey(
                        name: "FK_DANHGIA_HOADON_MaHd",
                        column: x => x.MaHd,
                        principalTable: "HOADON",
                        principalColumn: "MaHD",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DANHGIA_KHACHHANG_MaKh",
                        column: x => x.MaKh,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaCombo",
                table: "DANHGIA",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaCtsp",
                table: "DANHGIA",
                column: "MaCtsp");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaHd",
                table: "DANHGIA",
                column: "MaHd");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_MaKh",
                table: "DANHGIA",
                column: "MaKh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DANHGIA");
        }
    }
}
