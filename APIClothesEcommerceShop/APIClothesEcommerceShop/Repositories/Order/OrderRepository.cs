using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.ComboDetails_Orders;
using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.DTO.OrderDetails;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Repositories.Order
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EcommerceShopContext db;

        public OrderRepository(EcommerceShopContext db)
        {
            this.db = db;
        }

        public async Task<Hoadon> CreateOrder(Hoadon model)
        {
            db.Hoadons.Add(model);
            await db.SaveChangesAsync();
            return model;
        }

        public async Task CancelOrders(int orderId, string selectedCancelStatus, string reasonCancel)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var existingHoaDon = await db.Hoadons.FindAsync(orderId);
                if (existingHoaDon == null)
                {
                    throw new Exception($"Không tìm thấy Hoadon với Id {orderId}");
                }

                existingHoaDon.TinhTrang = selectedCancelStatus;
                existingHoaDon.LyDoHuy = reasonCancel;

                var checkDetailOrder = await db.Cthoadons.Where(p => p.MaHd == existingHoaDon.MaHd).ToListAsync();
                if (!checkDetailOrder.Any())
                {
                    throw new Exception($"Không tìm thấy CTHoadon với Id {existingHoaDon.MaHd}");
                }

                foreach (var detail in checkDetailOrder)
                {
                    if (detail.MaCombo == null)
                    {
                        var findDetailproduct = await db.Chitietsanphams.FindAsync(detail.MaCtsp);
                        if (findDetailproduct != null)
                        {
                            findDetailproduct.SoLuongTon += detail.SoLuong;
                        }
                    }
                    else
                    {
                        var checkDetailOrderCombo = await db.Chitietcombohoadons
                            .Where(p => p.MaHd == existingHoaDon.MaHd && p.MaCombo == detail.MaCombo)
                            .ToListAsync();

                        foreach (var detailComboOder in checkDetailOrderCombo)
                        {
                            var findDetailproduct = await db.Chitietsanphams.FindAsync(detailComboOder.MaCtsp);
                            if (findDetailproduct != null)
                            {
                                findDetailproduct.SoLuongTon += detailComboOder.SoLuong;
                            }
                        }

                        var findCombo = await db.Combos.FindAsync(detail.MaCombo);
                        if (findCombo != null)
                        {
                            findCombo.SoLuong += detail.SoLuong;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(existingHoaDon.MaCode))
                {
                    var findCoupon = await db.Macoupons.FirstOrDefaultAsync(p => p.MaCode == existingHoaDon.MaCode);
                    if (findCoupon != null)
                    {
                        findCoupon.SoLuongDaDung -= 1;
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Lỗi", ex);
            }
        }

        public async Task<List<OrderResponseDTO>> GetAll(string? search, string? filter)
        {
            var query = GetOrderQuery();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.MaHd.ToString().Contains(search) || p.HoTen.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(p => p.TinhTrang.ToLower().Contains(filter.ToLower()));
            }

            var orders = await query.ToListAsync();
            return orders.Select(MapToOrderResponseDTO).ToList();
        }

        public async Task UpdateStatusOrders(int id, string status, int? MaNv, string paymentmethod, string? reasonCancel)
        {
            var findOrder = await db.Hoadons.FindAsync(id);
            if (findOrder == null)
            {
                throw new Exception("Not found Order");
            }

            if (status.ToLower() != "chờ xác nhận")
            {
                findOrder.MaNv = MaNv;
            }

            if (status.ToLower() == "đã giao cho đơn vị vận chuyển")
            {
                findOrder.BatDauGiao = DateTime.Now;
            }

            if (paymentmethod.ToLower() == "cod")
            {
                if (findOrder.NgayNhan == null && (status.ToLower() == "đã nhận" || status.ToLower() == "đã thanh toán"))
                {
                    findOrder.NgayNhan = DateTime.Now;
                }
                if (findOrder.NgayThanhToan == null && status.ToLower() == "đã thanh toán")
                {
                    findOrder.NgayThanhToan = DateTime.Now;
                }
            }
            else if (paymentmethod.ToLower() == "vnpay")
            {
                if (findOrder.NgayNhan == null && status.ToLower() == "đã nhận")
                {
                    findOrder.NgayNhan = DateTime.Now;
                }
            }

            if (status.ToLower() == "đã hủy" || status.ToLower() == "hoàn trả/hoàn tiền")
            {
                await CancelOrders(id, status, reasonCancel);
            }

            findOrder.TinhTrang = status;
            await db.SaveChangesAsync();
        }

        public async Task<OrderResponseDTO> GetbyId(int id)
        {
            var order = await GetOrderQuery().FirstOrDefaultAsync(p => p.MaHd == id);
            return order == null ? null : MapToOrderResponseDTO(order);
        }

        public async Task<List<OrderResponseDTO>> GetByMakh(int Makh, string? search, string? filter)
        {
            var query = GetOrderQuery().Where(p => p.MaKh == Makh).OrderByDescending(p => p.MaHd);

            if (!string.IsNullOrEmpty(search))
            {
                query = (IOrderedQueryable<Hoadon>)query.Where(p => p.MaHd.ToString().Contains(search) || p.HoTen.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter))
            {
                query = (IOrderedQueryable<Hoadon>)query.Where(p => p.TinhTrang.ToLower().Contains(filter.ToLower()));
            }

            var orders = await query.ToListAsync();
            return orders.Select(MapToOrderResponseDTO).ToList();
        }

        private IQueryable<Hoadon> GetOrderQuery()
        {
            return db.Hoadons
                .AsNoTracking()
                .Include(p => p.MaKhNavigation)
                .Include(p => p.MaCodeNavigation)
                .Include(p => p.MaNvNavigation)
                .Include(p => p.Chitietcombohoadons)
                    .ThenInclude(p => p.MaCtspNavigation)
                        .ThenInclude(p => p.MaSpNavigation)
                .Include(p => p.Cthoadons)
                    .ThenInclude(ct => ct.MaCtspNavigation)
                        .ThenInclude(ctsp => ctsp.MaSpNavigation)
                .Include(p => p.Cthoadons)
                    .ThenInclude(p => p.MaComboNavigation);
        }

        private OrderResponseDTO MapToOrderResponseDTO(Hoadon order)
        {
            return new OrderResponseDTO
            {
                MaHd = order.MaHd,
                MaKh = order.MaKh,
                TenKh = order.MaKhNavigation.HoTen,
                MaNv = order.MaNv,
                TenNv = order.MaNvNavigation?.HoTen,
                MaCode = order.MaCode,
                NgayNhan = order.NgayNhan,
                NgayTao = order.NgayTao,
                NgayThanhToan = order.NgayThanhToan,
                BatDauGiao = order.BatDauGiao,
                DiaChiNhanHang = order.DiaChiNhanHang,
                HinhThucTt = order.HinhThucTt,
                TinhTrang = order.TinhTrang,
                MoTa = order.MoTa,
                HoTen = order.HoTen,
                Sdt = order.Sdt,
                LyDoHuy = order.LyDoHuy,
                PhiVanChuyen = order.PhiVanChuyen,
                TienGoc = order.TienGoc,
                GiamGiaCoupon = order.MaCodeNavigation != null
                    ? (order.MaCodeNavigation.SoTienGiam > 0
                        ? order.MaCodeNavigation.SoTienGiam
                        : (order.MaCodeNavigation.PhanTramGiam * order.TienGoc / 100))
                    : 0m,
                Chitietcombohoadons = order.Chitietcombohoadons.Select(ctcb => new ComboDetails_OrdersResponseDTO
                {
                    MaHd = ctcb.MaHd,
                    MaCtsp = ctcb.MaCtsp,
                    TenSanPham = ctcb.MaCtspNavigation.MaSpNavigation.TenSanPham,
                    MauSac = ctcb.MaCtspNavigation.MauSac,
                    KichThuoc = ctcb.MaCtspNavigation.KichThuoc,
                    MaCombo = ctcb.MaCombo,
                    SoLuong = ctcb.SoLuong,
                    DonGia = ctcb.DonGia,
                }).ToList(),
                Cthoadons = order.Cthoadons.Select(cthd => new OrderDetailsResponseDTO
                {
                    Id = cthd.Id,
                    TenSanPham = cthd.MaCtspNavigation?.MaSpNavigation?.TenSanPham,
                    TenCombo = cthd.MaComboNavigation?.TenCombo,
                    BienThe = cthd.MaCtspNavigation != null
                        ? $"Màu: {cthd.MaCtspNavigation.MauSac} - Kích thước: {cthd.MaCtspNavigation.KichThuoc}"
                        : null,
                    MaHd = cthd.MaHd,
                    MaCtsp = cthd.MaCtsp,
                    MaCombo = cthd.MaCombo,
                    SoLuong = cthd.SoLuong,
                    Gia = cthd.Gia,
                    GiamGia = cthd.GiamGia,
                    GiaGoc = cthd.Gia + (cthd.GiamGia ?? 0),
                }).ToList()
            };
        }
    }
}
