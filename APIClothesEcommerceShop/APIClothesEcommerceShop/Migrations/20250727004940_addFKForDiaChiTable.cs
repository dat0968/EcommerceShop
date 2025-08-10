using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class addFKForDiaChiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaKh",
                table: "DIACHI",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DIACHI_MaKh",
                table: "DIACHI",
                column: "MaKh");

            migrationBuilder.AddForeignKey(
                name: "FK__DiaChi__MaKH__6750009E",
                table: "DIACHI",
                column: "MaKh",
                principalTable: "KHACHHANG",
                principalColumn: "MaKH",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__DiaChi__MaKH__6750009E",
                table: "DIACHI");

            migrationBuilder.DropIndex(
                name: "IX_DIACHI_MaKh",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "MaKh",
                table: "DIACHI");
        }
    }
}
