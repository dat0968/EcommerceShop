using System;
using System.Collections.Generic;
using System.Linq;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class OrderWithReview
    {
        public int MaHd { get; set; }

        public int? MaKh { get; set; }

        public int? MaNv { get; set; }

        public string? MaCode { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? BatDauGiao { get; set; }

        public DateTime? NgayNhan { get; set; }

        public string DiaChiNhanHang { get; set; } = null!;

        public DateTime? NgayThanhToan { get; set; }

        public string HinhThucTt { get; set; } = null!;

        public string TinhTrang { get; set; } = null!;

        public string? MoTa { get; set; }

        public string HoTen { get; set; } = null!;

        public string Sdt { get; set; } = null!;

        public string? LyDoHuy { get; set; }

        public bool? IsActive { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TienGoc { get; set; }

        public List<ProductInOrderWithReview> Products { get; set; } = new List<ProductInOrderWithReview>();

        public List<ComboInOrderWithReview> Combos { get; set; } = new List<ComboInOrderWithReview>();

        public bool HasReviews => Products.Any(p => p.SoSao > 0) || Combos.Any(c => c.SoSao > 0);
    }

    public class ProductInOrderWithReview
    {
        public int Id { get; set; }

        public int MaSp { get; set; }

        public int? MaCtsp { get; set; }

        public int SoLuong { get; set; }

        public int Gia { get; set; }

        public int GiamGia { get; set; }

        public int MaDanhGia { get; set; }

        public string NoiDung { get; set; } = string.Empty;

        public int SoSao { get; set; }

        public bool HasReview => SoSao > 0;
    }

    public class ComboInOrderWithReview
    {
        public int MaCtsp { get; set; }

        public int MaCombo { get; set; }

        public int SoLuong { get; set; }

        public int DonGia { get; set; }

        public int MaDanhGia { get; set; }

        public string NoiDung { get; set; } = string.Empty;

        public int SoSao { get; set; }

        public bool HasReview => SoSao > 0;
    }

    public static class OrderWithReviewExtensions
    {
        public static OrderWithReview ToOrderWithReview(this Hoadon order)
        {
            return new OrderWithReview
            {
                MaHd = order.MaHd,
                MaKh = order.MaKh,
                MaNv = order.MaNv,
                MaCode = order.MaCode,
                NgayTao = order.NgayTao,
                BatDauGiao = order.BatDauGiao,
                NgayNhan = order.NgayNhan,
                DiaChiNhanHang = order.DiaChiNhanHang,
                NgayThanhToan = order.NgayThanhToan,
                HinhThucTt = order.HinhThucTt,
                TinhTrang = order.TinhTrang,
                MoTa = order.MoTa,
                HoTen = order.HoTen,
                Sdt = order.Sdt,
                LyDoHuy = order.LyDoHuy,
                IsActive = order.IsActive ?? true,
                PhiVanChuyen = order.PhiVanChuyen,
                TienGoc = order.TienGoc
            };
        }

        public static ProductInOrderWithReview ToProductInOrderWithReview(this Cthoadon product, Chitietsanpham? productDt, int MaKh)
        {
            return CreateProductReview(product, productDt, MaKh);
        }

        public static ComboInOrderWithReview ToComboInOrderWithReview(this Chitietcombohoadon comboHd, Combo combo, int MaKh)
        {
            return CreateComboReview(comboHd, combo, MaKh);
        }

        private static ProductInOrderWithReview CreateProductReview(Cthoadon product, Chitietsanpham? productDt, int MaKh)
        {
            var danhGia = productDt?.MaSpNavigation.DanhGias.FirstOrDefault(x => x.MaKh == MaKh) ?? new DanhGia();
            return new ProductInOrderWithReview
            {
                Id = product.Id,
                MaSp = product.MaCtspNavigation?.MaSp ?? 0,
                MaCtsp = product.MaCtsp,
                SoLuong = product.SoLuong,
                Gia = product.Gia,
                GiamGia = product.GiamGia,
                MaDanhGia = danhGia?.Id ?? 0,
                NoiDung = danhGia?.NoiDung ?? "Bạn chưa đánh giá.",
                SoSao = danhGia?.SoSao ?? 0
            };
        }

        private static ComboInOrderWithReview CreateComboReview(Chitietcombohoadon comboHd, Combo combo, int MaKh)
        {
            var danhGia = combo.DanhGias.FirstOrDefault(x => x.MaKh == MaKh);
            return new ComboInOrderWithReview
            {
                MaCtsp = comboHd.MaCtsp,
                MaCombo = comboHd.MaCombo,
                SoLuong = comboHd.SoLuong,
                DonGia = comboHd.DonGia,
                MaDanhGia = danhGia?.Id ?? 0,
                NoiDung = danhGia?.NoiDung ?? "Bạn chưa đánh giá.",
                SoSao = danhGia?.SoSao ?? 0
            };
        }
    }
}
