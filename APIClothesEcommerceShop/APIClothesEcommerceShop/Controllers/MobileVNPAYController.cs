using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileVNPAYController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly CheckoutService checkoutService;
        private readonly IOrderRepository orderRepository;
        private readonly ILogger<MobileVNPAYController> _logger; // Thêm logger

        public MobileVNPAYController(
            IVnpay vnpay,
            IConfiguration configuration,
            CheckoutService checkoutService,
            IOrderRepository orderRepository,
            ILogger<MobileVNPAYController> logger) // Thêm logger vào constructor
        {
            _vnpay = vnpay;
            _configuration = configuration;
            this.checkoutService = checkoutService;
            this.orderRepository = orderRepository;
            _logger = logger; // Khởi tạo logger

            // Debug configuration values
            var tmnCode = _configuration["Vnpay:TmnCode"];
            var hashSecret = _configuration["Vnpay:HashSecret"];
            var baseUrl = _configuration["Vnpay:BaseUrl"];
            var mobileReturnUrl = _configuration["Vnpay:MobileReturnUrl"];

            _logger.LogInformation("🔧 Mobile VNPay Configuration Debug:");
            _logger.LogInformation($"   TmnCode: {tmnCode ?? "NULL"}");
            _logger.LogInformation($"   HashSecret: {(string.IsNullOrEmpty(hashSecret) ? "NULL" : "***HIDDEN***")}");
            _logger.LogInformation($"   BaseUrl: {baseUrl ?? "NULL"}");
            _logger.LogInformation($"   MobileReturnUrl: {mobileReturnUrl ?? "NULL"}");

            // Validate configuration before initializing
            if (string.IsNullOrEmpty(tmnCode))
                throw new ArgumentNullException("Vnpay:TmnCode", "VNPay TmnCode is missing in configuration");

            if (string.IsNullOrEmpty(hashSecret))
                throw new ArgumentNullException("Vnpay:HashSecret", "VNPay HashSecret is missing in configuration");

            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException("Vnpay:BaseUrl", "VNPay BaseUrl is missing in configuration");

            if (string.IsNullOrEmpty(mobileReturnUrl))
                throw new ArgumentNullException("Vnpay:MobileReturnUrl", "VNPay MobileReturnUrl is missing in configuration");

            // Initialize VNPay with mobile-specific return URL
            try
            {
                _vnpay.Initialize(tmnCode, hashSecret, baseUrl, mobileReturnUrl);
                _logger.LogInformation("✅ Mobile VNPay initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Mobile VNPay initialization failed: {ex.Message}");
                throw;
            }
        }

        [HttpPost("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreateMobilePaymentUrl([FromBody] OrderRequestDTO model)
        {
            Hoadon newOrder = null;
            try
            {
                _logger.LogInformation("📱 Mobile VNPay CreatePaymentUrl called");
                _logger.LogInformation($"📱 Request from: {Request.Headers["User-Agent"]}");

                // Tạo order như bình thường
                newOrder = await checkoutService.Checkout(model);
                if (newOrder == null)
                {
                    throw new Exception("Failed to create order for mobile payment");
                }

                _logger.LogInformation($"📱 Order created: {newOrder.MaHd}");

                // Lấy IP address
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                // Tính tổng tiền
                var totalAmount = (double)(model.TienGoc + model.PhiVanChuyen - (model.GiamGia ?? 0));

                _logger.LogInformation($"📱 Payment amount: {totalAmount} VND");

                // Tạo payment request với mobile-specific settings
                var request = new PaymentRequest
                {
                    PaymentId = newOrder.MaHd,
                    Money = totalAmount,
                    Description = $"Mobile payment - Order #{newOrder.MaHd} - {model.MoTa}",
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                _logger.LogInformation($"📱 Payment request: {System.Text.Json.JsonSerializer.Serialize(request)}");

                // Tạo payment URL
                var paymentUrl = _vnpay.GetPaymentUrl(request);

                _logger.LogInformation($"📱 Mobile VNPay URL created: {paymentUrl}");

                // Return với response object để mobile dễ parse
                var response = new
                {
                    paymentUrl = paymentUrl,
                    orderId = newOrder.MaHd,
                    amount = totalAmount,
                    message = "Mobile payment URL created successfully",
                    timestamp = DateTime.Now
                };

                return Created(paymentUrl, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Mobile VNPay CreatePaymentUrl Error: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");

                // Cleanup order nếu có lỗi
                if (newOrder != null && newOrder.MaHd > 0)
                {
                    await orderRepository.CancelOrders(newOrder.MaHd, "Đã hủy", "Mobile VNPay payment creation failed");
                }

                return BadRequest(new
                {
                    error = "Failed to create mobile payment URL",
                    details = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("MobileCallback")]
        public async Task<ActionResult> MobileCallback()
        {
            _logger.LogInformation("📱 Mobile VNPay Callback received via ngrok");
            _logger.LogInformation($"📱 Full URL: {Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");

            // Log all query parameters for debugging
            _logger.LogInformation($"📱 Query Parameters Count: {Request.Query.Count}");
            foreach (var param in Request.Query)
            {
                _logger.LogInformation($"📱 Query Param: {param.Key} = {param.Value}");
            }

            if (!Request.QueryString.HasValue || Request.Query.Count == 0)
            {
                _logger.LogWarning("❌ No query parameters found in mobile callback");

                var mobileFrontendUrl = _configuration["App:MobileFrontendUrl"] ?? "capacitor://localhost";
                var errorUrl = $"{mobileFrontendUrl}/payment-result?" +
                              $"status=error&" +
                              $"error=no_payment_data&" +
                              $"message={Uri.EscapeDataString("Không có thông tin thanh toán từ VNPay")}";

                return Redirect(errorUrl);
            }

            try
            {
                // Manual signature validation with detailed logging
                var vnpHashSecret = _configuration["Vnpay:HashSecret"];
                var vnpSecureHash = Request.Query["vnp_SecureHash"].ToString();

                _logger.LogInformation($"📱 VNPay HashSecret from config: {(string.IsNullOrEmpty(vnpHashSecret) ? "NULL" : "***SET***")}");
                _logger.LogInformation($"📱 VNPay SecureHash from callback: {vnpSecureHash}");

                // Create hash string for validation
                var hashData = new StringBuilder();
                var sortedParams = Request.Query
                    .Where(kv => !string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash")
                    .OrderBy(kv => kv.Key, StringComparer.Ordinal);

                foreach (var param in sortedParams)
                {
                    if (hashData.Length > 0)
                        hashData.Append('&');
                    hashData.Append($"{param.Key}={param.Value}");
                }

                var dataToHash = hashData.ToString();
                _logger.LogInformation($"📱 Data to hash: {dataToHash}");

                // Calculate expected hash
                var expectedHash = CalculateVNPayHash(dataToHash, vnpHashSecret);
                _logger.LogInformation($"📱 Expected hash: {expectedHash}");
                _logger.LogInformation($"📱 Received hash: {vnpSecureHash}");
                _logger.LogInformation($"📱 Hash match: {expectedHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase)}");

                // Get payment parameters
                var responseCode = Request.Query["vnp_ResponseCode"].ToString();
                var transactionStatus = Request.Query["vnp_TransactionStatus"].ToString();
                var orderIdParam = Request.Query["vnp_TxnRef"].ToString();
                var amount = Request.Query["vnp_Amount"].ToString();
                var transactionNo = Request.Query["vnp_TransactionNo"].ToString();
                var payDate = Request.Query["vnp_PayDate"].ToString();

                // Parse order ID
                if (!int.TryParse(orderIdParam, out int orderId))
                {
                    _logger.LogError($"❌ Invalid order ID: {orderIdParam}");
                    var mobileFrontendUrl = _configuration["App:MobileFrontendUrl"] ?? "capacitor://localhost";
                    var errorUrl = $"{mobileFrontendUrl}/payment-result?" +
                                  $"status=error&" +
                                  $"error=invalid_order_id&" +
                                  $"message={Uri.EscapeDataString("Mã đơn hàng không hợp lệ")}";
                    return Redirect(errorUrl);
                }

                _logger.LogInformation($"📱 Processing payment: OrderId={orderId}, ResponseCode={responseCode}, TransactionStatus={transactionStatus}");

                // Try to process with VNPay library first
                PaymentResult paymentResult = null;
                try
                {
                    paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    _logger.LogInformation($"📱 VNPay library validation: SUCCESS");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"📱 VNPay library validation failed: {ex.Message}");

                    // Manual processing if library fails
                    paymentResult = new PaymentResult
                    {
                        IsSuccess = responseCode == "00" && transactionStatus == "00",
                        PaymentId = orderId
                    };

                    _logger.LogInformation($"📱 Manual processing: OrderId={orderId}, Success={paymentResult.IsSuccess}");
                }

                var mobileFrontendUrl2 = _configuration["App:MobileFrontendUrl"] ?? "capacitor://localhost";

                // Check if payment was successful
                bool isPaymentSuccess = (paymentResult?.IsSuccess == true) || (responseCode == "00" && transactionStatus == "00");

                if (isPaymentSuccess)
                {
                    // Verify order exists
                    var order = await orderRepository.GetbyId(orderId);
                    if (order == null)
                    {
                        _logger.LogError($"❌ Order {orderId} not found in database");
                        var notFoundUrl = $"{mobileFrontendUrl2}/payment-result?" +
                                        $"status=error&" +
                                        $"error=order_not_found&" +
                                        $"orderId={orderId}&" +
                                        $"message={Uri.EscapeDataString("Không tìm thấy đơn hàng trong hệ thống")}";
                        return Redirect(notFoundUrl);
                    }

                    // Update order status to confirmed
                    await orderRepository.UpdateStatusOrders(
                        orderId,
                        "Chờ xác nhận",
                        null,
                        "VNPAY_MOBILE_NGROK",
                        transactionNo
                    );

                    _logger.LogInformation($"✅ Order {orderId} updated to 'Chờ xác nhận' with transaction: {transactionNo}");

                    // Success redirect to mobile app
                    var successUrl = $"{mobileFrontendUrl2}/payment-result?" +
                                   $"status=success&" +
                                   $"vnp_ResponseCode={responseCode}&" +
                                   $"vnp_TransactionStatus={transactionStatus}&" +
                                   $"orderId={orderId}&" +
                                   $"amount={amount}&" +
                                   $"transactionNo={transactionNo}&" +
                                   $"payDate={payDate}&" +
                                   $"message={Uri.EscapeDataString("Thanh toán thành công!")}";

                    _logger.LogInformation($"📱 Redirecting to mobile app success: {successUrl}");
                    return Redirect(successUrl);
                }
                else
                {
                    // Payment failed - cancel order
                    await orderRepository.CancelOrders(orderId, "Đã hủy", $"VNPay payment failed via ngrok - Code: {responseCode}");
                    _logger.LogWarning($"❌ Order {orderId} cancelled due to payment failure - Code: {responseCode}");

                    // Get error message
                    string errorMessage = GetVNPayErrorMessage(responseCode);

                    // Failed redirect to mobile app
                    var failedUrl = $"{mobileFrontendUrl2}/payment-result?" +
                                  $"status=failed&" +
                                  $"vnp_ResponseCode={responseCode}&" +
                                  $"vnp_TransactionStatus={transactionStatus}&" +
                                  $"orderId={orderId}&" +
                                  $"error={Uri.EscapeDataString(errorMessage)}&" +
                                  $"message={Uri.EscapeDataString("Thanh toán không thành công")}";

                    _logger.LogInformation($"📱 Redirecting to mobile app failed: {failedUrl}");
                    return Redirect(failedUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Mobile VNPay Callback Error: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");

                var mobileFrontendUrl = _configuration["App:MobileFrontendUrl"] ?? "capacitor://localhost";
                var errorUrl = $"{mobileFrontendUrl}/payment-result?" +
                             $"status=error&" +
                             $"error=processing_exception&" +
                             $"message={Uri.EscapeDataString("Lỗi xử lý thanh toán trên server")}";

                return Redirect(errorUrl);
            }
        }

        private string CalculateVNPayHash(string data, string secretKey)
        {
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(secretKey);
                var dataBytes = Encoding.UTF8.GetBytes(data);

                using (var hmac = new System.Security.Cryptography.HMACSHA512(keyBytes))
                {
                    var hashBytes = hmac.ComputeHash(dataBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error calculating hash: {ex.Message}");
                return "";
            }
        }

        // Helper method to get VNPay error messages
        private string GetVNPayErrorMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                _ => "Giao dịch thất bại"
            };
        }

        // Health check endpoint cho mobile - với debug info
        [HttpGet("Health")]
        public ActionResult CheckHealth()
        {
            try
            {
                var healthResponse = new
                {
                    status = "Mobile VNPay service is running",
                    timestamp = DateTime.Now,
                    environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                    server = Environment.MachineName,
                    configuration = new
                    {
                        tmnCodeExists = !string.IsNullOrEmpty(_configuration["Vnpay:TmnCode"]),
                        hashSecretExists = !string.IsNullOrEmpty(_configuration["Vnpay:HashSecret"]),
                        baseUrlExists = !string.IsNullOrEmpty(_configuration["Vnpay:BaseUrl"]),
                        mobileReturnUrlExists = !string.IsNullOrEmpty(_configuration["Vnpay:MobileReturnUrl"]),
                        mobileFrontendUrlExists = !string.IsNullOrEmpty(_configuration["App:MobileFrontendUrl"])
                    },
                    vnpayInitialized = true // Nếu đến được đây thì VNPay đã init thành công
                };

                _logger.LogInformation($"📱 Health check OK: {System.Text.Json.JsonSerializer.Serialize(healthResponse)}");
                return Ok(healthResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Health check error: {ex.Message}");
                return StatusCode(500, new
                {
                    status = "Mobile VNPay service error",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        // Debug endpoint để kiểm tra configuration
        [HttpGet("Debug/Config")]
        public ActionResult GetDebugConfig()
        {
            if (_configuration["ASPNETCORE_ENVIRONMENT"] != "Development")
            {
                return NotFound();
            }

            var debugInfo = new
            {
                timestamp = DateTime.Now,
                configuration = new
                {
                    vnpay_TmnCode = _configuration["Vnpay:TmnCode"] ?? "NOT_SET",
                    vnpay_HashSecret = string.IsNullOrEmpty(_configuration["Vnpay:HashSecret"]) ? "NOT_SET" : "***HIDDEN***",
                    vnpay_BaseUrl = _configuration["Vnpay:BaseUrl"] ?? "NOT_SET",
                    vnpay_ReturnUrl = _configuration["Vnpay:ReturnUrl"] ?? "NOT_SET",
                    vnpay_MobileReturnUrl = _configuration["Vnpay:MobileReturnUrl"] ?? "NOT_SET",
                    app_MobileFrontendUrl = _configuration["App:MobileFrontendUrl"] ?? "NOT_SET"
                },
                allVnpayKeys = _configuration.GetSection("Vnpay").GetChildren().Select(x => new {
                    Key = x.Key,
                    HasValue = !string.IsNullOrEmpty(x.Value)
                }).ToList(),
                allAppKeys = _configuration.GetSection("App").GetChildren().Select(x => new {
                    Key = x.Key,
                    HasValue = !string.IsNullOrEmpty(x.Value)
                }).ToList()
            };

            return Ok(debugInfo);
        }
        // Thêm method này vào MobileVNPAYController.cs

        [HttpGet("CheckOrderStatus/{orderId}")]
        public async Task<ActionResult> CheckOrderStatus(int orderId)
        {
            try
            {
                _logger.LogInformation($"📱 Checking order status for Order {orderId}");

                var order = await orderRepository.GetbyId(orderId);
                if (order == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Đơn hàng không tồn tại",
                        orderId = orderId
                    });
                }

                // Determine payment status based on order status
                bool isPaid = order.TinhTrang == "Chờ xác nhận" ||
                             order.TinhTrang == "Đã xác nhận" ||
                             order.TinhTrang == "Đang giao hàng" ||
                             order.TinhTrang == "Đã giao hàng";

                bool isCancelled = order.TinhTrang == "Đã hủy";
                bool isPending = order.TinhTrang == "Chờ thanh toán";

                string status;
                if (isPaid)
                    status = "success";
                else if (isCancelled)
                    status = "failed";
                else
                    status = "pending";

                var response = new
                {
                    success = true,
                    orderId = orderId,
                    status = status,
                    orderStatus = order.TinhTrang,
                    paymentMethod = order.HinhThucTt,
                    isPaid = isPaid,
                    isCancelled = isCancelled,
                    isPending = isPending,
                    totalAmount = order.TienGoc,
                    timestamp = DateTime.Now
                };

                _logger.LogInformation($"📱 Order {orderId} status: {order.TinhTrang}, Payment status: {status}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking order status: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi kiểm tra trạng thái đơn hàng",
                    error = ex.Message
                });
            }
        }
    }
}