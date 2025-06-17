using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class addincreaseId_GIOHANGCTCOMBO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa khóa chính hiện tại
            migrationBuilder.DropPrimaryKey(
                name: "PK__GIOHANGC__3214EC276D2330CA",
                table: "GIOHANGCTCOMBO");

            // Bước 2: Xóa cột ID cũ
            migrationBuilder.DropColumn(
                name: "ID",
                table: "GIOHANGCTCOMBO");

            // Bước 3: Thêm lại cột ID có Identity
            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "GIOHANGCTCOMBO",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Bước 4: Đặt lại ID làm Primary Key
            migrationBuilder.AddPrimaryKey(
                name: "PK__GIOHANGC__3214EC276D2330CA",
                table: "GIOHANGCTCOMBO",
                column: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa khóa chính
            migrationBuilder.DropPrimaryKey(
                name: "PK__GIOHANGC__3214EC276D2330CA",
                table: "GIOHANGCTCOMBO");

            // Bước 2: Xóa cột ID có Identity
            migrationBuilder.DropColumn(
                name: "ID",
                table: "GIOHANGCTCOMBO");

            // Bước 3: Thêm lại cột ID không Identity
            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "GIOHANGCTCOMBO",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Bước 4: Đặt lại ID làm Primary Key
            migrationBuilder.AddPrimaryKey(
                name: "PK__GIOHANGC__3214EC276D2330CA",
                table: "GIOHANGCTCOMBO",
                column: "ID");
        }
    }
}
