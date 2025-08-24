﻿using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.DTO.VNPAY;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Repositories.VNPAY;
using APIClothesEcommerceShop.Services;
using Azure;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPAYController : ControllerBase
    {
        private readonly IVnPayService _vnpay;
        private readonly IConfiguration _configuration;
        private readonly CheckoutService checkoutService;
        private readonly IOrderRepository orderRepository;
        public VNPAYController(IVnPayService vnpay, IConfiguration configuration, IOrderRepository orderRepository, CheckoutService checkoutService)
        {
            this.checkoutService = checkoutService;
            this.orderRepository = orderRepository;
            _vnpay = vnpay;
            _configuration = configuration;
        }
        [HttpPost("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreatePaymentUrl(OrderRequestDTO model)
        {
            Hoadon NewOrder = null;
            try
            {
                NewOrder = await checkoutService.Checkout(model);

                var request = new PaymentInformationModel
                {
                    ID = NewOrder.MaHd.ToString(),
                    Amount = (double)(model.TienGoc + model.PhiVanChuyen - model.GiamGia),
                    OrderDescription = $"{(double)(model.TienGoc + model.PhiVanChuyen - model.GiamGia)}",
                    OrderType = "VNPAY"
                };

                var paymentUrl = _vnpay.CreatePaymentUrl(request, HttpContext);

                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                if (NewOrder != null && NewOrder.MaHd > 0)
                {
                    await orderRepository.CancelOrders(NewOrder.MaHd, "Đã hủy", "Khách hủy giao dịch VNPAY (lỗi hệ thống)");
                }
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Callback")]
        public async Task<ActionResult<string>> Callback()
        {
            var paymentResult = _vnpay.PaymentExecute(Request.Query);
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var resultDescription = $"{paymentResult.OrderDescription}. {paymentResult.OrderDescription}.";

                    if (paymentResult.Success)
                    {
                        var FindOrder = await orderRepository.GetbyId(int.Parse(paymentResult.OrderId));
                        if (FindOrder == null)
                        {
                            throw new Exception("Order Not Found");
                        }
                        await orderRepository.UpdateStatusOrders(int.Parse(paymentResult.OrderId), "Chờ xác nhận", null, "VNPAY", null);
                        return Redirect($"https://angelfashion.io.vn/VNPAYresponse/{paymentResult.OrderId}/{paymentResult.OrderDescription}");
                    }
                    await orderRepository.CancelOrders(int.Parse(paymentResult.OrderId), "Đã hủy", "Khách hủy giao dịch VNPAY");
                    return BadRequest(resultDescription);
                }
                catch (Exception ex)
                {
                    await orderRepository.CancelOrders(int.Parse(paymentResult.OrderId), "Đã hủy", "Khách hủy giao dịch VNPAY");
                    return BadRequest(ex.Message);
                }
            }
            await orderRepository.CancelOrders(int.Parse(paymentResult.OrderId), "Đã hủy", "Khách hủy giao dịch VNPAY");
            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}