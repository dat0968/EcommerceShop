using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class addComboIdToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BINHLUAN_SANPHAM_IdSanPham",
                table: "BINHLUAN");

            migrationBuilder.AlterColumn<int>(
                name: "IdSanPham",
                table: "BINHLUAN",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IdCombo",
                table: "BINHLUAN",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BINHLUAN_IdCombo",
                table: "BINHLUAN",
                column: "IdCombo");

            migrationBuilder.AddForeignKey(
                name: "FK_BINHLUAN_COMBO_IdCombo",
                table: "BINHLUAN",
                column: "IdCombo",
                principalTable: "COMBO",
                principalColumn: "MaCombo");

            migrationBuilder.AddForeignKey(
                name: "FK_BINHLUAN_SANPHAM_IdSanPham",
                table: "BINHLUAN",
                column: "IdSanPham",
                principalTable: "SANPHAM",
                principalColumn: "MaSP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BINHLUAN_COMBO_IdCombo",
                table: "BINHLUAN");

            migrationBuilder.DropForeignKey(
                name: "FK_BINHLUAN_SANPHAM_IdSanPham",
                table: "BINHLUAN");

            migrationBuilder.DropIndex(
                name: "IX_BINHLUAN_IdCombo",
                table: "BINHLUAN");

            migrationBuilder.DropColumn(
                name: "IdCombo",
                table: "BINHLUAN");

            migrationBuilder.AlterColumn<int>(
                name: "IdSanPham",
                table: "BINHLUAN",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BINHLUAN_SANPHAM_IdSanPham",
                table: "BINHLUAN",
                column: "IdSanPham",
                principalTable: "SANPHAM",
                principalColumn: "MaSP",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
