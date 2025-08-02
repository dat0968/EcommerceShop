using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class FixNameColumn_TruyCapLanCuoi_Khachhang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TruyCapLlanCuoi",
                table: "KHACHHANG",
                newName: "TruyCapLanCuoi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TruyCapLanCuoi",
                table: "KHACHHANG",
                newName: "TruyCapLlanCuoi");
        }
    }
}
