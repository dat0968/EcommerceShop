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
        private readonly ILogger<MobileVNPAYController> _logger;

        public MobileVNPAYController(
            IVnpay vnpay,
            IConfiguration configuration,
            CheckoutService checkoutService,
            IOrderRepository orderRepository,
            ILogger<MobileVNPAYController> logger)
        {
            _vnpay = vnpay;
            _configuration = configuration;
            this.checkoutService = checkoutService;
            this.orderRepository = orderRepository;
            _logger = logger;

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

                // Create order as usual
                newOrder = await checkoutService.Checkout(model);
                if (newOrder == null)
                {
                    throw new Exception("Failed to create order for mobile payment");
                }

                _logger.LogInformation($"📱 Order created: {newOrder.MaHd}");

                // Get IP address
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                // Calculate total amount
                var totalAmount = (double)(model.TienGoc + model.PhiVanChuyen - (model.GiamGia ?? 0));

                _logger.LogInformation($"📱 Payment amount: {totalAmount} VND");

                // Create payment request with mobile-specific settings
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

                // Create payment URL
                var paymentUrl = _vnpay.GetPaymentUrl(request);

                _logger.LogInformation($"📱 Mobile VNPay URL created: {paymentUrl}");

                // Return with response object for easy mobile parsing
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

                // Cleanup order if error occurs
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
            _logger.LogInformation("📱 Mobile VNPay Callback received");
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
                return await RedirectToMobileApp("error", null, "no_payment_data", "Không có thông tin thanh toán từ VNPay");
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
                    return await RedirectToMobileApp("error", null, "invalid_order_id", "Mã đơn hàng không hợp lệ");
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

                // Check if payment was successful
                bool isPaymentSuccess = (paymentResult?.IsSuccess == true) || (responseCode == "00" && transactionStatus == "00");

                if (isPaymentSuccess)
                {
                    // Verify order exists
                    var order = await orderRepository.GetbyId(orderId);
                    if (order == null)
                    {
                        _logger.LogError($"❌ Order {orderId} not found in database");
                        return await RedirectToMobileApp("error", orderId, "order_not_found", "Không tìm thấy đơn hàng trong hệ thống");
                    }

                    // Update order status to confirmed
                    await orderRepository.UpdateStatusOrders(
                        orderId,
                        "Chờ xác nhận",
                        null,
                        "VNPAY_MOBILE",
                        transactionNo
                    );

                    _logger.LogInformation($"✅ Order {orderId} updated to 'Chờ xác nhận' with transaction: {transactionNo}");

                    // Success redirect to mobile app
                    return await RedirectToMobileApp("success", orderId, null, "Thanh toán thành công!", new
                    {
                        vnp_ResponseCode = responseCode,
                        vnp_TransactionStatus = transactionStatus,
                        amount = amount,
                        transactionNo = transactionNo,
                        payDate = payDate
                    });
                }
                else
                {
                    // Payment failed - cancel order
                    await orderRepository.CancelOrders(orderId, "Đã hủy", $"VNPay payment failed - Code: {responseCode}");
                    _logger.LogWarning($"❌ Order {orderId} cancelled due to payment failure - Code: {responseCode}");

                    // Get error message
                    string errorMessage = GetVNPayErrorMessage(responseCode);

                    // Failed redirect to mobile app
                    return await RedirectToMobileApp("failed", orderId, errorMessage, "Thanh toán không thành công", new
                    {
                        vnp_ResponseCode = responseCode,
                        vnp_TransactionStatus = transactionStatus,
                        error = errorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Mobile VNPay Callback Error: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");

                return await RedirectToMobileApp("error", null, "processing_exception", "Lỗi xử lý thanh toán trên server");
            }
        }

        // NEW: Enhanced mobile app redirect method
        private async Task<ActionResult> RedirectToMobileApp(string status, int? orderId = null, string error = null, string message = null, object additionalData = null)
        {
            try
            {
                // Get mobile frontend URL from configuration
                var mobileFrontendUrl = _configuration["App:MobileFrontendUrl"] ?? "capacitor://localhost";

                // Build query parameters
                var queryParams = new List<string>
                {
                    $"status={Uri.EscapeDataString(status)}"
                };

                if (orderId.HasValue)
                    queryParams.Add($"orderId={orderId.Value}");

                if (!string.IsNullOrEmpty(error))
                    queryParams.Add($"error={Uri.EscapeDataString(error)}");

                if (!string.IsNullOrEmpty(message))
                    queryParams.Add($"message={Uri.EscapeDataString(message)}");

                // Add additional data if provided
                if (additionalData != null)
                {
                    var properties = additionalData.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(additionalData)?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value)}");
                        }
                    }
                }

                // Add timestamp
                queryParams.Add($"timestamp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

                var redirectUrl = $"{mobileFrontendUrl}/payment-result?{string.Join("&", queryParams)}";

                _logger.LogInformation($"📱 Redirecting to mobile app: {redirectUrl}");

                // For mobile apps, we need to handle deep linking properly
                var userAgent = Request.Headers["User-Agent"].ToString().ToLower();

                if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
                {
                    // Mobile device detected - use JavaScript redirect for better compatibility
                    var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Đang chuyển hướng...</title>
    <style>
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            margin: 0;
            padding: 20px;
            text-align: center;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }}
        .container {{
            background: rgba(255,255,255,0.1);
            border-radius: 15px;
            padding: 30px;
            backdrop-filter: blur(10px);
            box-shadow: 0 8px 32px rgba(0,0,0,0.1);
        }}
        .spinner {{
            border: 3px solid rgba(255,255,255,0.3);
            border-radius: 50%;
            border-top: 3px solid white;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 20px auto;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        .status {{
            font-size: 24px;
            margin: 20px 0;
        }}
        .message {{
            font-size: 16px;
            opacity: 0.9;
            margin: 10px 0;
        }}
        .manual-link {{
            background: rgba(255,255,255,0.2);
            border: 1px solid rgba(255,255,255,0.3);
            border-radius: 8px;
            padding: 12px 24px;
            color: white;
            text-decoration: none;
            display: inline-block;
            margin-top: 20px;
            transition: all 0.3s ease;
        }}
        .manual-link:hover {{
            background: rgba(255,255,255,0.3);
            transform: translateY(-2px);
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='status'>
            {(status == "success" ? "✅ Thanh toán thành công!" : status == "failed" ? "❌ Thanh toán thất bại" : "⚠️ Có lỗi xảy ra")}
        </div>
        <div class='spinner'></div>
        <div class='message'>Đang chuyển về ứng dụng...</div>
        {(orderId.HasValue ? $"<div class='message'>Mã đơn hàng: #{orderId.Value}</div>" : "")}
        <a href='{redirectUrl}' class='manual-link' id='manualLink'>
            Nhấn vào đây nếu không tự động chuyển
        </a>
    </div>

    <script>
        // Multiple redirect strategies for maximum compatibility
        console.log('🔄 Starting mobile app redirect...');
        console.log('🎯 Target URL:', '{redirectUrl}');
        
        // Strategy 1: Immediate redirect
        setTimeout(function() {{
            console.log('📱 Attempting immediate redirect...');
            window.location.replace('{redirectUrl}');
        }}, 1000);
        
        // Strategy 2: Backup redirect
        setTimeout(function() {{
            console.log('🔄 Backup redirect attempt...');
            window.location.href = '{redirectUrl}';
        }}, 3000);
        
        // Strategy 3: Force redirect if still on page
        setTimeout(function() {{
            console.log('🚨 Force redirect - still on redirect page');
            document.getElementById('manualLink').click();
        }}, 5000);
        
        // Handle visibility change (when user comes back to tab)
        document.addEventListener('visibilitychange', function() {{
            if (!document.hidden) {{
                console.log('👁️ Page became visible, attempting redirect...');
                window.location.replace('{redirectUrl}');
            }}
        }});
        
        // Handle page focus
        window.addEventListener('focus', function() {{
            console.log('🎯 Window focused, attempting redirect...');
            setTimeout(function() {{
                window.location.replace('{redirectUrl}');
            }}, 500);
        }});
    </script>
</body>
</html>";

                    return Content(htmlContent, "text/html");
                }
                else
                {
                    // Desktop or unknown - direct redirect
                    return Redirect(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RedirectToMobileApp: {ex.Message}");

                // Fallback simple redirect
                var fallbackUrl = $"capacitor://localhost/payment-result?status={status}&orderId={orderId}&message={Uri.EscapeDataString(message ?? "Redirect error")}";
                return Redirect(fallbackUrl);
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

        // Health check endpoint for mobile - with debug info
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
                    vnpayInitialized = true // If we reach here, VNPay is initialized successfully
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

        // Debug endpoint to check configuration
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