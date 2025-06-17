using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPAYController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly CheckoutService checkoutService;
        private readonly IOrderRepository orderRepository;
        public VNPAYController(IVnpay vnpay, IConfiguration configuration, IOrderRepository orderRepository, CheckoutService checkoutService)
        {
            this.checkoutService = checkoutService;
            this.orderRepository = orderRepository;
            _vnpay = vnpay;
            _configuration = configuration;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:ReturnUrl"]);
        }
        [HttpPost("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreatePaymentUrl(OrderRequestDTO model)
        {
            var NewOrder = await checkoutService.Checkout(model);
            try
            {
                
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext); // Lấy địa chỉ IP của thiết bị thực hiện giao dịch

                var request = new PaymentRequest
                {
                    PaymentId = NewOrder.MaHd,
                    Money = (double)(model.TienGoc + model.PhiVanChuyen - model.GiamGia),
                    Description = $"{(double)(model.TienGoc + model.PhiVanChuyen - model.GiamGia)}",
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                    CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                    Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                    Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);

                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                await orderRepository.CancelOrders((int)NewOrder.MaHd, "Đã hủy", "Khách hủy giao dịch VNPAY");
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Callback")]
        public async Task<ActionResult<string>> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                try
                {
                    var resultDescription = $"{paymentResult.PaymentResponse.Description}. {paymentResult.TransactionStatus.Description}.";

                    if (paymentResult.IsSuccess)
                    {
                        var FindOrder = await orderRepository.GetbyId((int)paymentResult.PaymentId);
                        if (FindOrder == null)
                        {
                            throw new Exception("Order Not Found");
                        }
                        await orderRepository.UpdateStatusOrders((int)paymentResult.PaymentId, "Chờ xác nhận", null, "VNPAY", null);
                        return Redirect($"http://localhost:5173/VNPAYresponse/{paymentResult.PaymentId}/{paymentResult.Description}");
                    }
                    await orderRepository.CancelOrders((int)paymentResult.PaymentId, "Đã hủy", "Khách hủy giao dịch VNPAY");
                    return BadRequest(resultDescription);
                }
                catch (Exception ex)
                {
                    await orderRepository.CancelOrders((int)paymentResult.PaymentId, "Đã hủy", "Khách hủy giao dịch VNPAY");
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}
