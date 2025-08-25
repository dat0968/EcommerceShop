﻿using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Cart;
using APIClothesEcommerceShop.Repositories.Cart_DetailCombo;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories.Customer;
using APIClothesEcommerceShop.Repositories.Macoupon;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Repositories.OrderComboDetails;
using APIClothesEcommerceShop.Repositories.OrderDetails;
using APIClothesEcommerceShop.Repositories.ProductDetails;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using MailKit.Security;
using MimeKit;
using System;
using System.ComponentModel.Design;
using System.Reactive.Subjects;

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
        private readonly IComboRepository ComboRepository;
        private readonly IOrderComboDetails DetailComboOrderRepository;
        private readonly IProductDetailsRepository productDetailsRepository;
        private readonly IMaCouponRepository maCouponRepository;
        private readonly IConfiguration _configuration;
        private readonly ICustomerRepository _customerRepository;
        public CheckoutService(IConfiguration _configuration, ICustomerRepository _customerRepository, ICartRepository cartRepository, EcommerceShopContext db, IOrderRepository orderRepository, IMaCouponRepository maCouponRepository, IProductDetailsRepository productDetailsRepository, IOrderComboDetails DetailComboOrderRepository, IOrderDetails orderDetailsRepository, IOrderComboDetails orderComboDetailsRepository, IComboRepository ComboRepository)
        {
            this.db = db;
            this.orderRepository = orderRepository;
            this.orderDetailsRepository = orderDetailsRepository;
            this.orderComboDetailsRepository = orderComboDetailsRepository;
            this.ComboRepository = ComboRepository;
            this.DetailComboOrderRepository = DetailComboOrderRepository;
            this.productDetailsRepository = productDetailsRepository;
            this.maCouponRepository = maCouponRepository;
            this.cartRepository = cartRepository;
            this._configuration = _configuration;
            this._customerRepository = _customerRepository;
        }
        public async Task<Hoadon> Checkout(OrderRequestDTO model)
        {
            try
            {
                await db.Database.BeginTransactionAsync();
                var tinhtrangthantoan = "Chờ xác nhận";
                if(model.HinhThucTt.ToLower() == "vnpay")
                {
                    tinhtrangthantoan = "Đang xử lý VNPAY";
                }
                if(model.HinhThucTt.ToLower() == "tại cửa hàng")
                {
                    tinhtrangthantoan = "Đã thanh toán";
                }
                // Tạo mới hóa đơn
                var NewOrder = new Hoadon
                {
                    MaKh = model.MaKh,
                    MaNv = null,
                    MaCode = string.IsNullOrEmpty(model.MaCode) == true ? null : model.MaCode,
                    NgayTao = DateTime.Now,
                    BatDauGiao = null,
                    NgayNhan = null,
                    DiaChiNhanHang = model.DiaChiNhanHang,
                    NgayThanhToan = model.HinhThucTt.ToLower() == "cod" ? null : DateTime.Now,
                    HinhThucTt = model.HinhThucTt,
                    TinhTrang = tinhtrangthantoan,
                    MoTa = model.MoTa,
                    Sdt = model.Sdt,
                    LyDoHuy = null,
                    PhiVanChuyen = model.PhiVanChuyen,
                    TienGoc = model.TienGoc,
                    IsActive = true,
                    HoTen = model.HoTen,
                };
                NewOrder = await orderRepository.CreateOrder(NewOrder);
                


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
                    /* Cập nhật số lượng Combo và sản phẩm và cập thông sản phẩm trong
                     combo vào chitietcombohoadon ( nếu có )*/
                    if(detail.MaCombo != null)
                    {
                        var FindCombo = await ComboRepository.GetById(detail.MaCombo.Value);
                        if(FindCombo != null)
                        {
                            // cập nhật số lượng combo
                            FindCombo.SoLuong = FindCombo.SoLuong - detail.SoLuong;
                            if (FindCombo.SoLuong < 0)
                            {
                                throw new Exception($"Số lượng còn lại của combo {FindCombo.TenCombo} (mã {FindCombo.MaCombo}) không đủ");
                            }
                            var updateCombo = new Combo
                            {
                                MaCombo = FindCombo.MaCombo,
                                TenCombo = FindCombo.TenCombo,
                                Hinh = FindCombo.Hinh,
                                SoLuong = FindCombo.SoLuong,
                                PhanTramGiam = FindCombo.PhanTramGiam,
                                SoTienGiam = FindCombo.SoTienGiam,
                                MoTa = FindCombo.MoTa,
                                NgayBatDau = FindCombo.NgayBatDau,
                                NgayKetThuc = FindCombo.NgayKetThuc,
                                IsActive = true,
                            };
                            await ComboRepository.EditCombo(updateCombo);
                            // Lọc DetailCombo_OrderResquests theo MaCombo của ModelDetailOrder
                            var filteredDetailComboOrders = model.Chitietcombohoadons
                                ?.Where(d => d.MaCombo == detail.MaCombo.Value)
                                .ToList();
                            if(filteredDetailComboOrders != null && filteredDetailComboOrders.Count() > 0)
                            {
                                foreach (var detailComboOrder in filteredDetailComboOrders)
                                {
                                    var NewModel = new Chitietcombohoadon
                                    {
                                        MaHd = NewOrder.MaHd,
                                        MaCombo = detailComboOrder.MaCombo,
                                        MaCtsp = detailComboOrder.MaCtsp,
                                        SoLuong = detailComboOrder.SoLuong,
                                        DonGia = detailComboOrder.DonGia
                                    };
                                    NewModel = await DetailComboOrderRepository.AddDetailComboOrder(NewModel);

                                    // Cập nhật lại số lượng sản phẩm 
                                    var UpdateProduct = await productDetailsRepository.GetDetailByMaCTSp(detailComboOrder.MaCtsp);
                                    if (UpdateProduct == null)
                                    {
                                        throw new Exception("Sản phẩm không tồn tại");
                                    }
                                    UpdateProduct.SoLuongTon = UpdateProduct.SoLuongTon - detailComboOrder.SoLuong;
                                    if (UpdateProduct.SoLuongTon < 0)
                                    {
                                        throw new Exception($"Số lượng còn lại của sản phẩm {UpdateProduct.MaSpNavigation.TenSanPham} ({UpdateProduct.MaSp}) không đủ để đáp ứng cho combo mã {NewModel.MaCombo}");
                                    }
                                    else
                                    {
                                        await productDetailsRepository.Update(UpdateProduct);
                                    }
                                }
                            }
                        }
                    }

                    // Cập nhật lại số lượng sản phẩm
                    else
                    {
                        // Cập nhật lại số lượng sản phẩm 
                        var UpdateProduct = await productDetailsRepository.GetDetailByMaCTSp(detail.MaCtsp.Value);
                        UpdateProduct.SoLuongTon = UpdateProduct.SoLuongTon - OrderDetails.SoLuong;
                        if (UpdateProduct.SoLuongTon < 0)
                        {
                            throw new Exception($"Sản phẩm {UpdateProduct.MaSpNavigation.TenSanPham} ({UpdateProduct.MaSp}) đã hết hàng");
                        }
                        else
                        {
                            await productDetailsRepository.Update(UpdateProduct);
                        }
                    }
                }

                // Cập nhật lại số lượng mã coupon
                if (!string.IsNullOrEmpty(model.MaCode))
                {
                    var FindCoupon = await maCouponRepository.GetById(model.MaCode);
                    if(FindCoupon != null)
                    {
                        FindCoupon.SoLuongDaDung++;
                        await maCouponRepository.Update(FindCoupon);
                    }
                    else
                    {
                        throw new Exception("CouponCode not Found");
                    }
                }

                // Xóa giỏ hàng của khách
                foreach (int cartid in model.GioHangId)
                {
                    await cartRepository.DeleteCart(cartid);
                }
                var customer = await _customerRepository.GetCustomerByIdAsync(model.MaKh);
                
               var emailMessage = new MimeMessage();
               emailMessage.From.Add(new MailboxAddress(_configuration["GoogleEmailSetting:Username"], "datntpk03691@gmail.com"));
               emailMessage.To.Add(new MailboxAddress("", customer.Email));
               emailMessage.Subject = $"XÁC NHẬN ĐẶT HÀNG THÀNH CÔNG - MÃ ĐƠN {NewOrder.MaHd}";
               emailMessage.Body = new TextPart("html")
               {
                   Text = $@"
                   <h2>Cảm ơn quý khách đã đặt hàng tại <b>Angel Fashion</b>!</h2>
                   <p>Đơn hàng của quý khách đã được tiếp nhận và đang chờ xử lý.</p>
        
                   <h3>Thông tin khách hàng</h3>
                   <p><b>Họ tên người nhận:</b> {model.HoTen}</p>
                   <p><b>Email người đặt:</b> {customer.Email}</p>

                   <h3>Thông tin đơn hàng</h3>
                   <p><b>Mã đơn hàng:</b> {NewOrder.MaHd}</p>
                   <p><b>Ngày đặt:</b> {DateTime.Now:dd/MM/yyyy HH:mm}</p>

                   <p>Chúng tôi sẽ sớm liên hệ để xác nhận và giao hàng trong thời gian sớm nhất.</p>
                   <br/>
                   <p>Trân trọng.</p>
    "
               };

               using (var client = new MailKit.Net.Smtp.SmtpClient())
               {
                   await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                   await client.AuthenticateAsync(
                       _configuration["GoogleEmailSetting:Email"],
                       _configuration["GoogleEmailSetting:Password"]
                   );
                   await client.SendAsync(emailMessage);
                   await client.DisconnectAsync(true);
               }
                await db.Database.CommitTransactionAsync();
                return NewOrder;
            }catch(Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception($"Error: {ex.Message}", ex);
            }
            
        }
    }
}