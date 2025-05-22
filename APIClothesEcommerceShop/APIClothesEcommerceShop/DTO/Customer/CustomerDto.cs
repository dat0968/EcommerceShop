using System.ComponentModel.DataAnnotations;

namespace APIClothesEcommerceShop.DTO.Customer
{
    public class CustomerDto
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public DateTime NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public string CCCD { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string TenTaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string? Hinh { get; set; }
        public IFormFile HinhDaiDien { get; set; }
        public string TinhTrang { get; set; }
        public bool IsActive { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
