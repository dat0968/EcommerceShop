using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Services;
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
    public class VnpayPaymentController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly CheckoutService checkoutService;
        private readonly IOrderRepository orderRepository;
        public VnpayPaymentController(IVnpay vnpay, IConfiguration configuration, CheckoutService checkoutService, IOrderRepository orderRepository)
        {
            _vnpay = vnpay;
            _configuration = configuration;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:ReturnUrl"]);
            this.checkoutService = checkoutService;
            this.orderRepository = orderRepository;
        }
        [HttpGet("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreatePaymentUrl(OrderRequestDTO model)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext); // Lấy địa chỉ IP của thiết bị thực hiện giao dịch
                var NewOder = await checkoutService.Checkout(model);
                if(NewOder == null)
                {
                    throw new Exception("Failed to Checkout");
                }
                var request = new PaymentRequest
                {
                    PaymentId = NewOder.MaHd,
                    Money = (double)(model.TienGoc + model.PhiVanChuyen - (model.GiamGia ?? 0)),
                    Description = model.MoTa,
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
                        return Ok(resultDescription);
                    }
                    await orderRepository.CancelOrders((int)paymentResult.PaymentId, "Đã hủy", "Thanh toán VNPAY không thành công");
                    return BadRequest(resultDescription);
                }
                catch (Exception ex)
                {
                    await orderRepository.CancelOrders((int)paymentResult.PaymentId, "Đã hủy", "Thanh toán VNPAY không thành công");
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}
