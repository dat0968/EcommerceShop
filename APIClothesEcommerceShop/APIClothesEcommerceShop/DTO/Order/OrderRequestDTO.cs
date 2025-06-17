using APIClothesEcommerceShop.DTO.ComboDetails_Orders;
using APIClothesEcommerceShop.DTO.OrderDetails;

namespace APIClothesEcommerceShop.DTO.Order
{
    public class OrderRequestDTO
    {
        public int MaKh { get; set; }

        //public int? MaNv { get; set; }
        //public string? TenNv { get; set; }
        public string? MaCode { get; set; }

        //public DateTime NgayTao { get; set; }
            
        //public DateTime? BatDauGiao { get; set; }

        //public DateTime? NgayNhan { get; set; }

        public string DiaChiNhanHang { get; set; }

        //public DateTime? NgayThanhToan { get; set; }

        public string HinhThucTt { get; set; }

        //public string TinhTrang { get; set; }

        public string? MoTa { get; set; }

        public string HoTen { get; set; }

        public string Sdt { get; set; }

        //public string? LyDoHuy { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TienGoc { get; set; }
        public decimal? GiamGia { get; set; }
        public int[] GioHangId { get; set; } = new int[0];

        public virtual ICollection<ComboDetails_OrdersRequestDTO> Chitietcombohoadons { get; set; } = new List<ComboDetails_OrdersRequestDTO>();

        public virtual ICollection<OrderDetailsRequestDTO> Cthoadons { get; set; } = new List<OrderDetailsRequestDTO>();
    }
}
