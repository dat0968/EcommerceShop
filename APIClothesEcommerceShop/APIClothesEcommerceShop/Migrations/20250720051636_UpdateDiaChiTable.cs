using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiaChiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__DiaChiCT__MaKH__6754599E",
                table: "DIACHI");

            migrationBuilder.DropIndex(
                name: "IX_DIACHI_MaKh",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "MaKh",
                table: "DIACHI");

            migrationBuilder.RenameColumn(
                name: "DiaChiChiTiet",
                table: "DIACHI",
                newName: "diachichitiet");

            migrationBuilder.AddColumn<string>(
                name: "Hoten",
                table: "DIACHI",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MacDinh",
                table: "DIACHI",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QuanHuyen",
                table: "DIACHI",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SDT",
                table: "DIACHI",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tinh",
                table: "DIACHI",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "XaPhuong",
                table: "DIACHI",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hoten",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "MacDinh",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "QuanHuyen",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "SDT",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "Tinh",
                table: "DIACHI");

            migrationBuilder.DropColumn(
                name: "XaPhuong",
                table: "DIACHI");

            migrationBuilder.RenameColumn(
                name: "diachichitiet",
                table: "DIACHI",
                newName: "DiaChiChiTiet");

            migrationBuilder.AddColumn<int>(
                name: "MaKh",
                table: "DIACHI",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DIACHI_MaKh",
                table: "DIACHI",
                column: "MaKh");

            migrationBuilder.AddForeignKey(
                name: "FK__DiaChiCT__MaKH__6754599E",
                table: "DIACHI",
                column: "MaKh",
                principalTable: "KHACHHANG",
                principalColumn: "MaKH");
        }
    }
}
