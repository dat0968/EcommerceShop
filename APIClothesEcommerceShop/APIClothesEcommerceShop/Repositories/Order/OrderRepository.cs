using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.ComboDetails_Orders;
using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.DTO.OrderDetails;
using APIClothesEcommerceShop.Models;
using iText.Kernel.Pdf.Canvas;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

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
            try
            {
                db.Hoadons.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        public async Task CancelOrders(int oderId, string selectedCancelStatus, string reasonCancel)
        {
            await db.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật trạng thái Đã hủy hoặc Hoàn trả/Hoàn tiền cho đơn hàng
                var existingHoaDon = db.Hoadons.Local.FirstOrDefault(p => p.MaHd == oderId) ?? await db.Hoadons.FindAsync(oderId);
                if (existingHoaDon == null)
                {
                    throw new Exception($"Không tìm thấy Hoadon với Id {oderId}");
                }
                existingHoaDon.TinhTrang = selectedCancelStatus;
                existingHoaDon.LyDoHuy = reasonCancel;
                db.Hoadons.Update(existingHoaDon);
                // Hoàn lại số lượng sản phẩm mua lẻ và theo combo trong hóa đơn
                var checkDetailOrder = await db.Cthoadons.Where(p => p.MaHd == existingHoaDon.MaHd).ToListAsync();
                if (!checkDetailOrder.Any())
                {
                    throw new Exception($"Không tìm thấy CTHoadon với Id {existingHoaDon.MaHd}");
                }
                foreach (var detail in checkDetailOrder)
                {
                    if (detail.MaCombo == null)
                    {
                        var findDetailproduct = db.Chitietsanphams.Local.FirstOrDefault(p => p.MaCtsp == detail.MaCtsp) ?? await db.Chitietsanphams.FindAsync(detail.MaCtsp);
                        if (findDetailproduct == null)
                        {
                            throw new Exception($"Không tìm thấy CTSP với Id {detail.MaCtsp}");
                        }
                        findDetailproduct.SoLuongTon += detail.SoLuong;
                        db.Chitietsanphams.Update(findDetailproduct);
                    }
                    else
                    {
                        //Hoàn lại số lượng sản phẩm mua trong combo trong hóa đơn
                        var checkDetailOrderCombo = await db.Chitietcombohoadons.Where(p => p.MaHd == existingHoaDon.MaHd && p.MaCombo == detail.MaCombo).ToListAsync();
                        foreach (var detailComboOder in checkDetailOrderCombo)
                        {
                            var findDetailproduct = db.Chitietsanphams.Local.FirstOrDefault(p => p.MaCtsp == detailComboOder.MaCtsp) ?? await db.Chitietsanphams.FindAsync(detailComboOder.MaCtsp);
                            if (findDetailproduct == null)
                            {
                                throw new Exception($"Không tìm thấy CTSP với Id {detailComboOder.MaCtsp}");
                            }
                            findDetailproduct.SoLuongTon += detailComboOder.SoLuong;
                            db.Chitietsanphams.Update(findDetailproduct);
                        }
                        //Hoàn lại số lượng combo
                        var findCombo = db.Combos.Local.FirstOrDefault(p => p.MaCombo == detail.MaCombo) ?? await db.Combos.FindAsync(detail.MaCombo);
                        if (findCombo == null)
                        {
                            throw new Exception($"Không tìm thấy combo với Id {detail.MaCombo}");
                        }
                        findCombo.SoLuong += detail.SoLuong;
                        db.Combos.Update(findCombo);
                    }
                }
                //Hoàn lại mã coupon
                if (!string.IsNullOrEmpty(existingHoaDon.MaCode))
                {
                    var findCoupon = db.Macoupons.Local.FirstOrDefault(p => p.MaCode == existingHoaDon.MaCode) ?? await db.Macoupons.FirstOrDefaultAsync(p => p.MaCode == existingHoaDon.MaCode);
                    if (findCoupon == null)
                    {
                        throw new Exception($"Không tìm mã coupon {existingHoaDon.MaCode}");
                    }
                    findCoupon.SoLuongDaDung -= 1;
                    db.Macoupons.Update(findCoupon);
                }
                await db.SaveChangesAsync();
                await db.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception("Lỗi", ex);
            }
        }


        public async Task<List<OrderResponseDTO>> GetAll(string? search, string? filter)
        {
            var ordersRaw = await db.Hoadons.AsNoTracking().Include(p => p.MaCodeNavigation).ToListAsync();
            var ListOrder = ordersRaw.Select(order => new OrderResponseDTO
            {
                MaHd = order.MaHd,
                MaKh = order.MaKh != null ? order.MaKh.Value : null,
                MaNv = order.MaNv,
                TenNv = order.MaNvNavigation != null ? order.MaNvNavigation.HoTen : null,
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
                        ? (order.MaCodeNavigation.SoTienGiam != null && order.MaCodeNavigation.SoTienGiam > 0 
                            ? order.MaCodeNavigation.SoTienGiam : (order.MaCodeNavigation.PhanTramGiam * order.TienGoc / 100))
                        : 0m,
                Chitietcombohoadons = order.Chitietcombohoadons.Select(ctcb => new ComboDetails_OrdersResponseDTO
                {
                    MaHd = ctcb.MaHd,
                    MaCtsp = ctcb.MaCtsp,
                    MaCombo = ctcb.MaCombo,
                    SoLuong = ctcb.SoLuong,
                    DonGia = ctcb.DonGia,
                }).ToList(),
                Cthoadons = order.Cthoadons.Select(cthd => new OrderDetailsResponseDTO
                {
                    Id = cthd.Id,
                    TenSanPham = cthd.MaCtspNavigation.MaSpNavigation.TenSanPham,
                    BienThe = $"Màu: {cthd.MaCtspNavigation.MauSac} - Kích thước: {cthd.MaCtspNavigation.KichThuoc}",
                    MaHd = cthd.MaHd,
                    MaCtsp = cthd.MaCtsp,
                    MaCombo = cthd.MaCombo,
                    SoLuong = cthd.SoLuong,
                    Gia = cthd.Gia,
                }).ToList()
            }).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                ListOrder = ListOrder.Where(p => p.MaHd.ToString().Contains(search.ToLower()) || p.HoTen.ToLower().Contains(search.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(filter))
            {
                ListOrder = ListOrder.Where(p => p.TinhTrang.ToLower().Contains(filter.ToLower())).ToList();
            }
            return ListOrder;
        }

        public async Task UpdateStatusOrders(int id, string status, int? MaNv, string paymentmethod, string? reasonCancel)
        {
            try
            {
                var FindOrder = await db.Hoadons.FindAsync(id);
                if (FindOrder == null)
                {
                    throw new Exception("Not found Order");
                }

                if(status.ToLower() != "chờ xác nhận")
                {
                    FindOrder.MaNv = MaNv;
                }
                if(status.ToLower() == "đã giao cho đơn vị vận chuyển")
                {
                    FindOrder.BatDauGiao = DateTime.Now;
                }
                if (paymentmethod.ToLower() == "cod")
                {
                    if (FindOrder.NgayNhan == null)
                    {
                        if (status.ToLower() == "đã nhận" || (status.ToLower() == "đã thanh toán"))
                        {
                            FindOrder.NgayNhan = DateTime.Now;
                        }
                    }
                    if(FindOrder.NgayThanhToan == null)
                    {
                        if (status.ToLower() == "đã thanh toán")
                        {
                            FindOrder.NgayThanhToan = DateTime.Now;
                        }
                    }
                }
                if (paymentmethod.ToLower() == "vnpay")
                {
                    if (FindOrder.NgayNhan == null)
                    {
                        if (status.ToLower() == "đã nhận")
                        {
                            FindOrder.NgayNhan = DateTime.Now;
                        }
                    }
                }
                if(paymentmethod.ToLower() == "đã hủy" || paymentmethod.ToLower() == "hoàn trả/hoàn tiền")
                {
                    await CancelOrders(id, paymentmethod, reasonCancel);
                }
                FindOrder.TinhTrang = status;
                db.Hoadons.Update(FindOrder);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }

        }

        public async Task<Hoadon> GetbyId(int id)
        {
            var findOrder = await db.Hoadons.AsNoTracking().FirstOrDefaultAsync(p => p.MaHd == id);
            if(findOrder == null)
            {
                throw new Exception("Not found Order");
            }
            return findOrder;
        }

        public async Task<List<OrderResponseDTO>> GetByMakh(int Makh, string? search, string? filter)
        {
            try
            {
                var ordersRaw = await db.Hoadons
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
                        .ThenInclude(p => p.MaComboNavigation)
                    .Where(p => p.MaKh == Makh).OrderByDescending(p => p.MaHd)
                    .ToListAsync(); 

                var ListOrder = ordersRaw.Select(order => new OrderResponseDTO
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
                        ? (order.MaCodeNavigation.SoTienGiam != null && order.MaCodeNavigation.SoTienGiam > 0
                            ? order.MaCodeNavigation.SoTienGiam : (order.MaCodeNavigation.PhanTramGiam * order.TienGoc / 100))
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
                        TenSanPham = cthd.MaCtspNavigation != null && cthd.MaCtspNavigation.MaSpNavigation != null
                        ? cthd.MaCtspNavigation.MaSpNavigation.TenSanPham
                        : null,
                        TenCombo = cthd.MaComboNavigation != null && cthd.MaComboNavigation.TenCombo != null
                        ? cthd.MaComboNavigation.TenCombo
                        : null,
                        BienThe = cthd.MaCtspNavigation != null
                        ? $"Màu: {cthd.MaCtspNavigation.MauSac} - Kích thước: {cthd.MaCtspNavigation.KichThuoc}"
                        : null,
                        MaHd = cthd.MaHd,
                        MaCtsp = cthd.MaCtsp,
                        MaCombo = cthd.MaCombo,
                        SoLuong = cthd.SoLuong,
                        Gia = cthd.Gia,
                        GiamGia = cthd.GiamGia,
                        GiaGoc = cthd.Gia + (decimal)(cthd.GiamGia != null && cthd.GiamGia > 0 ? cthd.GiamGia : 0),
                    }).ToList()
                }).ToList();

                if (!string.IsNullOrEmpty(search))
                {
                    ListOrder = ListOrder.Where(p => p.MaHd.ToString().Contains(search.ToLower()) || p.HoTen.ToLower().Contains(search.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    ListOrder = ListOrder.Where(p => p.TinhTrang.ToLower().Contains(filter.ToLower())).ToList();
                }
                return ListOrder;
            }
            catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
