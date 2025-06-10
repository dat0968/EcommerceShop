using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    public partial class refactor_autoincrementfotIdGioHang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Xóa FK từ GIOHANGCTCOMBO → GIOHANG
            migrationBuilder.DropForeignKey(
                name: "FK__GIOHANGCT__MaGio__76969D2E",
                table: "GIOHANGCTCOMBO");

            // 2. Xóa khóa chính cũ của GIOHANG
            migrationBuilder.DropPrimaryKey(
                name: "PK__GIOHANG__3214EC27D6272CAB",
                table: "GIOHANG");

            // 3. Xóa cột ID cũ
            migrationBuilder.DropColumn(
                name: "ID",
                table: "GIOHANG");

            // 4. Tạo lại cột ID mới có Identity
            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "GIOHANG",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 5. Tạo khóa chính mới
            migrationBuilder.AddPrimaryKey(
                name: "PK_GIOHANG",
                table: "GIOHANG",
                column: "ID");

            // 6. Tạo lại khóa ngoại từ GIOHANGCTCOMBO → GIOHANG
            migrationBuilder.AddForeignKey(
                name: "FK_GIOHANGCTCOMBO_GIOHANG_MaGioHang",
                table: "GIOHANGCTCOMBO",
                column: "MaGioHang",
                principalTable: "GIOHANG",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa khóa chính mới
            migrationBuilder.DropPrimaryKey(
                name: "PK_GIOHANG",
                table: "GIOHANG");

            // Xóa cột ID mới
            migrationBuilder.DropColumn(
                name: "ID",
                table: "GIOHANG");

            // Tạo lại cột ID cũ không Identity
            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "GIOHANG",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Đặt lại khóa chính như cũ
            migrationBuilder.AddPrimaryKey(
                name: "PK__GIOHANG__3214EC27D6272CAB",
                table: "GIOHANG",
                column: "ID");
        }
    }
}
