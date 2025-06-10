using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class TenHinhAnhColumnforGioHangTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenHinhAnh",
                table: "GIOHANG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenHinhAnh",
                table: "GIOHANG");
        }
    }
}
