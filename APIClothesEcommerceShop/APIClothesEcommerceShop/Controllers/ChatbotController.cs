using APIClothesEcommerceShop.Helper;
using APIClothesEcommerceShop.Repositories.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IProductRepository productRepository;
        public ChatbotController(IConfiguration configuration, IProductRepository productRepository)
        {
            this.configuration = configuration;
            this.productRepository = productRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Chatbot(string request)
        {
            var frontendOrigin = HttpContext.Request.Headers["Origin"].ToString();
            const string sessionKey = "chat_history";
            var history = HttpContext.Session.GetObjectFromJson<List<string>>(sessionKey) ?? new List<string>();
            history.Add("User: " + request);
            var products = await productRepository.GetAll(null, null, null, null, null);
            var productListText = string.Join("\n", products.Select(p =>
                $"Tên: {p.TenSanPham}, Mã: {p.MaSp}, Giá: {p.KhoangGia}, Link: {frontendOrigin}/product/{p.MaSp}"
            ));
            var prompt = $"""
            Bạn là chatbot thông minh của cửa hàng chúng tôi.
            Dựa vào lịch sử trò chuyện:
            {string.Join("\n", history)}

            và dữ liệu sản phẩm (dưới đây là thông tin các sản phẩm đang có, bao gồm cả link trực tiếp tới sản phẩm):

            {productListText}

            Hãy trả lời cho yêu cầu của người dùng một cách ngắn gọn, rõ ràng. Khi đề cập đến sản phẩm, hãy liệt kê sản phẩm theo danh sách bằng cách dùng <br/> chứ đừng ghi liền rất khó đọc, 
            tên sản phẩm in đậm bằng đặt chúng vào giữa theo cú pháp <strong>TenSanPham</strong> và
            hãy dẫn link HTML như đã cung cấp (ví dụ: <a target="_blank" href="...">Xem chi tiết</a>).
            """;
            var googleAI = new GoogleAI(apiKey: configuration["Chatbot:Gemini:GeminiAPIKey"]);
            var model = googleAI.GenerativeModel(model: Model.Gemini15FlashLatest);
            var response = await model.GenerateContent(prompt);
            history.Add("Bot: " + response.Text);
            HttpContext.Session.SetObjectAsJson(sessionKey, history);
            return Ok(response.Text);
        }
    }
}
