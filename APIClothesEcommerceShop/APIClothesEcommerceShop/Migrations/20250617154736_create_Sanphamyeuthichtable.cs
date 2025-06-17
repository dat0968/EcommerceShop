using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class create_Sanphamyeuthichtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SANPHAMYEUTHICH",
                columns: table => new
                {
                    MaSp = table.Column<int>(type: "int", nullable: false),
                    MaKh = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SANPHAMYEUTHICH", x => new { x.MaSp, x.MaKh });
                    table.ForeignKey(
                        name: "FK__SPYEUTHICH__MaKH__6754119E",
                        column: x => x.MaKh,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH");
                    table.ForeignKey(
                        name: "FK__SPYEUTHICH__MaKH__6754889E",
                        column: x => x.MaSp,
                        principalTable: "SANPHAM",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SANPHAMYEUTHICH_MaKh",
                table: "SANPHAMYEUTHICH",
                column: "MaKh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SANPHAMYEUTHICH");
        }
    }
}
