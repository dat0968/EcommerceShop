using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;

namespace APIClothesEcommerceShop.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly IConfiguration _configuration;

        public GeminiAIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ResponseAPI<object>> AnalyzeReviewContent(string reviewContent)
        {
            var response = new ResponseAPI<object>();
            try
            {
                // Get API key from configuration
                var apiKey = _configuration["Chatbot:Gemini:GeminiAPIKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    response.SetErrorResponse("Gemini API Key is not configured.");
                    return response;
                }

                // Prepare the prompt for content moderation
                var prompt = $"Kiểm tra nội dung đánh giá sau đây có phù hợp không: \"{reviewContent}\".\n" +
                             "Vui lòng trả lời với định dạng JSON như sau:\n" +
                             "{\n" +
                             "  \"IsAppropriate\": true/false,\n" +
                             "  \"Message\": \"Lý do nội dung không phù hợp (nếu có).\"\n" +
                             "}";

                // Initialize Google AI with the API key
                var googleAI = new GoogleAI(apiKey: apiKey);
                var model = googleAI.GenerativeModel(model: Model.Gemini15Flash);

                // Generate content based on the prompt
                var aiResponse = await model.GenerateContent(prompt);
                var responseText = aiResponse.Text;

                // Parse the JSON response by extracting content between first { and last }
                if (!string.IsNullOrEmpty(responseText))
                {
                    try
                    {
                        // Use regex to extract JSON content between first { and last }
                        var match = Regex.Match(responseText, @"\{.*\}");
                        if (match.Success)
                        {
                            var jsonContent = match.Value;
                            var aiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AiReviewResponse>(jsonContent);
                            if (aiResult != null)
                            {
                                if (aiResult.IsAppropriate)
                                {
                                    response.SetSuccessResponse(data: true, message: "Nội dung đánh giá phù hợp.");
                                }
                                else
                                {
                                    response.SetErrorResponse(aiResult.Message ?? "Nội dung đánh giá không phù hợp.");
                                }
                                return response;
                            }
                        }
                        else
                        {
                            response.SetErrorResponse("Không tìm thấy nội dung JSON hợp lệ trong phản hồi từ AI.");
                            return response;
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        response.SetErrorResponse($"Lỗi khi phân tích phản hồi JSON từ AI: {jsonEx.Message}");
                        return response;
                    }
                }
                response.SetErrorResponse("Không nhận được phản hồi hợp lệ từ dịch vụ AI.");
            }
            catch (Exception ex)
            {
                response.SetErrorResponse($"Lỗi ngoại lệ khi phân tích đánh giá bằng AI: {ex.Message}");
            }
            return response;
        }
    }

    public class AiReviewResponse
    {
        public bool IsAppropriate { get; set; }
        public string Message { get; set; } = string.Empty; // Optional: AI service might return a message
    }
}
