using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIClothesEcommerceShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePKforRefreshTokenValueGeneratedOnAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Drop khóa chính đang ràng buộc trên cột ID
                ALTER TABLE REFRESHTOKEN DROP CONSTRAINT PK__REFRESHT__3214EC27840D2D6F;

                -- 2. Thêm cột tạm có Identity
                ALTER TABLE REFRESHTOKEN ADD ID_temp INT IDENTITY(1,1) NOT NULL;

                -- 3. Xóa cột ID cũ
                ALTER TABLE REFRESHTOKEN DROP COLUMN ID;

                -- 4. Đổi tên cột tạm thành ID
                EXEC sp_rename 'REFRESHTOKEN.ID_temp', 'ID', 'COLUMN';

                -- 5. Tạo lại khóa chính trên cột ID mới
                ALTER TABLE REFRESHTOKEN ADD CONSTRAINT PK_REFRESH_TOKEN_ID PRIMARY KEY (ID);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Việc xoay ngược khá phức tạp, bạn có thể để trống hoặc làm tương tự nhưng bỏ identity
            ");
        }
    }
}
