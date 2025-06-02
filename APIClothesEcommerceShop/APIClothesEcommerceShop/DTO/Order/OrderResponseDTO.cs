using APIClothesEcommerceShop.DTO.ComboDetails_Orders;
using APIClothesEcommerceShop.DTO.OrderDetails;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Order
{
    public class OrderResponseDTO
    {
        public int MaHd { get; set; }

        public int? MaKh { get; set; }

        public int? MaNv { get; set; }
        public string? TenNv { get; set; }
        public string? MaCode { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? BatDauGiao { get; set; }

        public DateTime? NgayNhan { get; set; }

        public string DiaChiNhanHang { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        public string HinhThucTt { get; set; }

        public string TinhTrang { get; set; }

        public string? MoTa { get; set; }

        public string HoTen { get; set; }

        public string Sdt { get; set; }

        public string? LyDoHuy { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TienGoc { get; set; }

        public virtual ICollection<ComboDetails_OrdersResponseDTO> Chitietcombohoadons { get; set; } = new List<ComboDetails_OrdersResponseDTO>();

        public virtual ICollection<OrderDetailsResponseDTO> Cthoadons { get; set; } = new List<OrderDetailsResponseDTO>();
    }
}
