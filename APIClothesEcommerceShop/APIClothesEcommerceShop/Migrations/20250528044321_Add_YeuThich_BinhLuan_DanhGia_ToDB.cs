using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class Add_YeuThich_BinhLuan_DanhGia_ToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SANPHAM",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "COMBO",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BINHLUAN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSanPham = table.Column<int>(type: "int", nullable: false),
                    IdKhachHang = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayBinhLuan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BINHLUAN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BINHLUAN_KHACHHANG_IdKhachHang",
                        column: x => x.IdKhachHang,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BINHLUAN_SANPHAM_IdSanPham",
                        column: x => x.IdSanPham,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DANHGIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdKhachHang = table.Column<int>(type: "int", nullable: false),
                    IdSanPham = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(54)", maxLength: 54, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DANHGIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DANHGIA_KHACHHANG_IdKhachHang",
                        column: x => x.IdKhachHang,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DANHGIA_SANPHAM_IdSanPham",
                        column: x => x.IdSanPham,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YEUTHICH",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DaThich = table.Column<bool>(type: "bit", nullable: false),
                    IdKhachHang = table.Column<int>(type: "int", nullable: false),
                    IdDanhGia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YEUTHICH", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YEUTHICH_DANHGIA_IdDanhGia",
                        column: x => x.IdDanhGia,
                        principalTable: "DANHGIA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YEUTHICH_KHACHHANG_IdKhachHang",
                        column: x => x.IdKhachHang,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BINHLUAN_IdKhachHang",
                table: "BINHLUAN",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_BINHLUAN_IdSanPham",
                table: "BINHLUAN",
                column: "IdSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_IdKhachHang",
                table: "DANHGIA",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DANHGIA_IdSanPham",
                table: "DANHGIA",
                column: "IdSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_YEUTHICH_IdDanhGia",
                table: "YEUTHICH",
                column: "IdDanhGia");

            migrationBuilder.CreateIndex(
                name: "IX_YEUTHICH_IdKhachHang",
                table: "YEUTHICH",
                column: "IdKhachHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BINHLUAN");

            migrationBuilder.DropTable(
                name: "YEUTHICH");

            migrationBuilder.DropTable(
                name: "DANHGIA");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SANPHAM",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "COMBO",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
