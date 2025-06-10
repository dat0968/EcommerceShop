using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Cart;
using APIClothesEcommerceShop.Repositories.Cart_DetailCombo;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Repositories.OrderComboDetails;
using APIClothesEcommerceShop.Repositories.OrderDetails;
using System.ComponentModel.Design;

namespace APIClothesEcommerceShop.Services
{
    public class CheckoutService
    {
        private readonly EcommerceShopContext db;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderDetails orderDetailsRepository;
        private readonly IOrderComboDetails orderComboDetailsRepository;
        private readonly ICartRepository cartRepository;
        private readonly ICart_DetailComboRepository cart_DetailComboRepository;
        public CheckoutService(EcommerceShopContext db, IOrderRepository orderRepository, IOrderDetails orderDetailsRepository, IOrderComboDetails orderComboDetailsRepository)
        {
            this.db = db;
            this.orderRepository = orderRepository;
            this.orderDetailsRepository = orderDetailsRepository;
            this.orderComboDetailsRepository = orderComboDetailsRepository;
        }
        public async Task<Hoadon> Checkout(OrderRequestDTO model)
        {
            try
            {
                await db.Database.BeginTransactionAsync();
                // Tạo mới hóa đơn
                var NewOrder = new Hoadon
                {
                    MaKh = model.MaKh,
                    MaNv = model.MaNv,
                    MaCode = model.MaCode,
                    NgayTao = DateTime.Now,
                    BatDauGiao = model.BatDauGiao,
                    NgayNhan = model.NgayNhan,
                    DiaChiNhanHang = model.DiaChiNhanHang,
                    NgayThanhToan = model.NgayThanhToan,
                    HinhThucTt = model.HinhThucTt,
                    TinhTrang = model.TinhTrang,
                    MoTa = model.MoTa,
                    Sdt = model.Sdt,
                    LyDoHuy = model.LyDoHuy,
                    PhiVanChuyen = model.PhiVanChuyen,
                    TienGoc = model.TienGoc,
                    IsActive = true,
                    HoTen = model.HoTen,
                };
                NewOrder = await orderRepository.CreateOrder(NewOrder);
                await db.Database.CommitTransactionAsync();


                // Tạo chi tiết hóa đơn
                foreach(var detail in model.Cthoadons)
                {
                    var OrderDetails = new Cthoadon
                    {
                        MaHd = NewOrder.MaHd,
                        MaCtsp = detail.MaCtsp,
                        MaCombo = detail.MaCombo,
                        SoLuong = detail.SoLuong,
                        Gia = detail.Gia,
                        GiamGia = detail.GiamGia,
                    };
                    OrderDetails = await orderDetailsRepository.CreateOrderDetails(OrderDetails);
                }
                // Tạo mới thông tin chi tiết combo trong hóa đơn
                foreach (var combo in model.Chitietcombohoadons)
                {
                    var ComboDetails_Order = new Chitietcombohoadon
                    {
                        MaCombo = combo.MaCombo,
                        MaHd = NewOrder.MaHd,
                        MaCtsp = combo.MaCtsp,
                        SoLuong = combo.SoLuong,
                        DonGia = combo.DonGia,
                    };
                    ComboDetails_Order = await orderComboDetailsRepository.CreateComboOrderDetails(ComboDetails_Order);

                }
                // Cập nhật lại số lượng mã coupon


                // Xóa giỏ hàng của khách
                foreach(int cartid in model.GioHangId)
                {
                    await cartRepository.DeleteCart(cartid);
                }

                await db.Database.CommitTransactionAsync();
                return NewOrder;
            }catch(Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception("Error", ex);
            }
            
        }
    }
}
