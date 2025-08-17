using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class BinhLuanToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "BINHLUAN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSanPham = table.Column<int>(type: "int", nullable: true),
                    IdCombo = table.Column<int>(type: "int", nullable: true),
                    MaKh = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayBinhLuan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    LyDoHuy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BINHLUAN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BINHLUAN_COMBO_IdCombo",
                        column: x => x.IdCombo,
                        principalTable: "COMBO",
                        principalColumn: "MaCombo");
                    table.ForeignKey(
                        name: "FK_BINHLUAN_KHACHHANG_MaKh",
                        column: x => x.MaKh,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BINHLUAN_SANPHAM_IdSanPham",
                        column: x => x.IdSanPham,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BINHLUAN");
        }
    }
}
