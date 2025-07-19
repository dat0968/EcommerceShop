using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class addMaCombocolumnForLichSuXemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__LSXEM__MaKH__6751189E",
                table: "LICHSUXEM");

            migrationBuilder.AlterColumn<int>(
                name: "MaSp",
                table: "LICHSUXEM",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MaCombo",
                table: "LICHSUXEM",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LICHSUXEM_MaCombo",
                table: "LICHSUXEM",
                column: "MaCombo");

            migrationBuilder.AddForeignKey(
                name: "FK__LSXEM__MaCombo__6751189E",
                table: "LICHSUXEM",
                column: "MaCombo",
                principalTable: "COMBO",
                principalColumn: "MaCombo");

            migrationBuilder.AddForeignKey(
                name: "FK__LSXEM__MaSp__6751189E",
                table: "LICHSUXEM",
                column: "MaSp",
                principalTable: "SANPHAM",
                principalColumn: "MaSP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__LSXEM__MaCombo__6751189E",
                table: "LICHSUXEM");

            migrationBuilder.DropForeignKey(
                name: "FK__LSXEM__MaSp__6751189E",
                table: "LICHSUXEM");

            migrationBuilder.DropIndex(
                name: "IX_LICHSUXEM_MaCombo",
                table: "LICHSUXEM");

            migrationBuilder.DropColumn(
                name: "MaCombo",
                table: "LICHSUXEM");

            migrationBuilder.AlterColumn<int>(
                name: "MaSp",
                table: "LICHSUXEM",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__LSXEM__MaKH__6751189E",
                table: "LICHSUXEM",
                column: "MaSp",
                principalTable: "SANPHAM",
                principalColumn: "MaSP");
        }
    }
}
