using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class createLichSuXemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LICHSUXEM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKh = table.Column<int>(type: "int", nullable: false),
                    MaSp = table.Column<int>(type: "int", nullable: false),
                    ThoiGianXem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LICHSUXEM", x => x.Id);
                    table.ForeignKey(
                        name: "FK__LSXEM__MaKH__6224119E",
                        column: x => x.MaKh,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH");
                    table.ForeignKey(
                        name: "FK__LSXEM__MaKH__6751189E",
                        column: x => x.MaSp,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LICHSUXEM_MaKh",
                table: "LICHSUXEM",
                column: "MaKh");

            migrationBuilder.CreateIndex(
                name: "IX_LICHSUXEM_MaSp",
                table: "LICHSUXEM",
                column: "MaSp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LICHSUXEM");
        }
    }
}
