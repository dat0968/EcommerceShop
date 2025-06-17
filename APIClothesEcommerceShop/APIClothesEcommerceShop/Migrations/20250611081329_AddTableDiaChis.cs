using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class AddTableDiaChis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DIACHI",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaChiChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKh = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DIACHI", x => x.ID);
                    table.ForeignKey(
                        name: "FK__DiaChiCT__MaKH__6754599E",
                        column: x => x.MaKh,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DIACHI_MaKh",
                table: "DIACHI",
                column: "MaKh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DIACHI");
        }
    }
}
