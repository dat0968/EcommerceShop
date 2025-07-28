using System;
using System.Collections.Generic;
using System.Net.Http;
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
                var prompt = $"Kiểm tra nội dung đánh giá sau đây có chứa các từ ngữ thô lỗ gây phản cảm không không: \"{reviewContent}\".\n" +
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
                        var match = Regex.Match(responseText, @"```json([\s\S]*?)```");
                        if (match.Success)
                        {
                            var jsonContent = match.Groups[1].Value;
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

        public async Task<ResponseAPI<object>> AnalyzeImageForComparison(List<string> imageUrls)
        {
            var response = new ResponseAPI<object>();
            try
            {
                var apiKey = _configuration["Chatbot:Gemini:GeminiAPIKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    response.SetErrorResponse("Gemini API Key is not configured.");
                    return response;
                }

                var googleAI = new GoogleAI(apiKey: apiKey);
                var model = googleAI.GenerativeModel(model: Model.Gemini15Flash);

                var results = new List<object>();

                using (var httpClient = new HttpClient())
                {
                    foreach (var imageUrl in imageUrls)
                    {
                        try
                        {
                            // Download image from URL
                            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                            // Determine MIME type based on URL extension or default to jpeg
                            var mimeType = "image/jpeg";
                            if (imageUrl.ToLower().Contains(".png"))
                                mimeType = "image/png";
                            else if (imageUrl.ToLower().Contains(".gif"))
                                mimeType = "image/gif";
                            else if (imageUrl.ToLower().Contains(".webp"))
                                mimeType = "image/webp";

                            var promptText = @"Phân tích hình ảnh này về:
                                1. Chất lượng hình ảnh (độ nét, ánh sáng, góc chụp)
                                2. Màu sắc và tông màu chủ đạo
                                3. Phong cách và đặc điểm nổi bật
                                4. Đánh giá tổng thể về tính thẩm mỹ

                                Trả lời bằng tiếng Việt với định dạng JSON như sau:
                                {
                                ""quality_score"": (điểm từ 1-10),
                                ""dominant_colors"": [""màu1"", ""màu2""],
                                ""style"": ""mô tả phong cách"",
                                ""aesthetic_features"": ""các đặc điểm thẩm mỹ nổi bật"",
                                ""overall_rating"": (điểm từ 1-10)
                                }";

                            // Create content with image and prompt - use simple approach like in ChatbotController
                            var base64Image = Convert.ToBase64String(imageBytes);
                            var combinedPrompt = $"data:{mimeType};base64,{base64Image}\n\n{promptText}";

                            var aiResponse = await model.GenerateContent(combinedPrompt);
                            var responseText = aiResponse.Text;

                            // Parse JSON response
                            if (!string.IsNullOrEmpty(responseText))
                            {
                                try
                                {
                                    // Extract JSON from markdown if present
                                    var jsonMatch = Regex.Match(responseText, @"```json([\s\S]*?)```");
                                    string jsonContent;
                                    if (jsonMatch.Success)
                                    {
                                        jsonContent = jsonMatch.Groups[1].Value.Trim();
                                    }
                                    else
                                    {
                                        // Try to find JSON object directly
                                        var directJsonMatch = Regex.Match(responseText, @"\{[\s\S]*\}");
                                        if (directJsonMatch.Success)
                                        {
                                            jsonContent = directJsonMatch.Value;
                                        }
                                        else
                                        {
                                            throw new Exception("No JSON object found in response");
                                        }
                                    }

                                    var analysisResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);

                                    results.Add(new
                                    {
                                        ImageUrl = imageUrl,
                                        Analysis = new
                                        {
                                            quality_score = (float)(analysisResult?.quality_score ?? 0),
                                            dominant_colors = analysisResult?.dominant_colors?.ToObject<string[]>() ?? new string[0],
                                            style = (string)(analysisResult?.style ?? "N/A"),
                                            aesthetic_features = (string)(analysisResult?.aesthetic_features ?? "N/A"),
                                            overall_rating = (float)(analysisResult?.overall_rating ?? 0)
                                        }
                                    });
                                }
                                catch (Exception parseEx)
                                {
                                    // Fallback: return raw response if JSON parsing fails
                                    results.Add(new
                                    {
                                        ImageUrl = imageUrl,
                                        Analysis = new
                                        {
                                            raw_response = responseText,
                                            parsing_error = parseEx.Message
                                        }
                                    });
                                }
                            }
                            else
                            {
                                results.Add(new { ImageUrl = imageUrl, Error = "Không nhận được phản hồi từ AI" });
                            }
                        }
                        catch (HttpRequestException httpEx)
                        {
                            results.Add(new { ImageUrl = imageUrl, Error = $"Lỗi khi tải hình ảnh: {httpEx.Message}" });
                        }
                        catch (Exception imgEx)
                        {
                            results.Add(new { ImageUrl = imageUrl, Error = $"Lỗi khi phân tích hình ảnh: {imgEx.Message}" });
                        }
                    }
                }

                response.SetSuccessResponse(data: results, message: "Phân tích hình ảnh hoàn tất.");
            }
            catch (Exception ex)
            {
                response.SetErrorResponse($"Lỗi ngoại lệ khi phân tích hình ảnh bằng AI: {ex.Message}");
            }
            return response;
        }

        public async Task<ResponseAPI<object>> AnalyzeTryOnImageAsync(string imageBase64, List<object> productsData)
        {
            var response = new ResponseAPI<object>();
            try
            {
                var apiKey = _configuration["Chatbot:Gemini:GeminiAPIKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    response.SetErrorResponse("Gemini API Key is not configured.");
                    return response;
                }

                var googleAI = new GoogleAI(apiKey: apiKey);
                var model = googleAI.GenerativeModel(model: Model.Gemini15Flash);

                // Convert base64 to Part for Gemini
                var imageBytes = Convert.FromBase64String(imageBase64.Split(',')[1]); // Remove data:image/jpeg;base64, prefix

                // Build prompt with product information
                var productDetails = "";
                if (productsData != null && productsData.Count > 0)
                {
                    productDetails = "Thông tin sản phẩm gốc:\n";
                    foreach (var product in productsData)
                    {
                        var productJson = Newtonsoft.Json.JsonConvert.SerializeObject(product);
                        var productObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(productJson);

                        if (productObj?.type == "combo")
                        {
                            productDetails += $"Combo: {productObj?.comboName}\n";
                            productDetails += $"Mô tả: {productObj?.description}\n";
                            if (productObj?.products != null)
                            {
                                productDetails += "Các sản phẩm:\n";
                                foreach (var prod in productObj.products)
                                {
                                    productDetails += $"  - {prod?.name} ({prod?.variant?.color}, {prod?.variant?.size})\n";
                                }
                            }
                        }
                        else
                        {
                            productDetails += $"Sản phẩm: {productObj?.name}\n";
                            productDetails += $"Danh mục: {productObj?.category}\n";
                            productDetails += $"Mô tả: {productObj?.description}\n";
                            productDetails += $"Màu sắc: {productObj?.variant?.color}\n";
                            productDetails += $"Kích thước: {productObj?.variant?.size}\n";
                        }
                        productDetails += "\n";
                    }
                }

                var promptText = $@"Dựa trên thông tin sản phẩm gốc sau:

                    {productDetails}

                    Phân tích tính thẩm mỹ, phong cách và sự phù hợp giới tính của trang phục được người mẫu mặc trong hình ảnh đã cung cấp. Xem xét mức độ phù hợp của trang phục với người mẫu và tổng thể hình ảnh. Cung cấp điểm thẩm mỹ trên thang điểm 10, và mô tả phong cách cũng như sự phù hợp giới tính. 

                    Định dạng phản hồi dưới dạng đối tượng JSON với các khóa: aesthetic_score (float), style (string), gender_suitability (string).

                    Ví dụ:
                    {{
                    ""aesthetic_score"": 8.5,
                    ""style"": ""Hiện đại, trẻ trung"",
                    ""gender_suitability"": ""Phù hợp với nam giới""
                    }}";

                // Use simple approach like in ChatbotController
                var base64ImageForTryOn = Convert.ToBase64String(imageBytes);
                var combinedPromptForTryOn = $"data:image/jpeg;base64,{base64ImageForTryOn}\n\n{promptText}";

                var aiResponse = await model.GenerateContent(combinedPromptForTryOn);
                var responseText = aiResponse.Text;

                // Parse JSON response
                if (!string.IsNullOrEmpty(responseText))
                {
                    try
                    {
                        // Extract JSON from markdown if present
                        var jsonMatch = Regex.Match(responseText, @"```json([\s\S]*?)```");
                        string jsonContent;
                        if (jsonMatch.Success)
                        {
                            jsonContent = jsonMatch.Groups[1].Value.Trim();
                        }
                        else
                        {
                            // Try to find JSON object directly
                            var directJsonMatch = Regex.Match(responseText, @"\{[\s\S]*\}");
                            if (directJsonMatch.Success)
                            {
                                jsonContent = directJsonMatch.Value;
                            }
                            else
                            {
                                throw new Exception("No JSON object found in response");
                            }
                        }

                        var analysisResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);

                        var result = new
                        {
                            aesthetic_score = (float)(analysisResult?.aesthetic_score ?? 0),
                            style = (string)(analysisResult?.style ?? "N/A"),
                            gender_suitability = (string)(analysisResult?.gender_suitability ?? "N/A")
                        };

                        response.SetSuccessResponse(data: result, message: "Phân tích hình ảnh thử đồ hoàn tất.");
                    }
                    catch (Exception parseEx)
                    {
                        // Fallback: extract information heuristically
                        var aestheticMatch = Regex.Match(responseText, @"aesthetic_score[""']?\s*:\s*([\d.]+)", RegexOptions.IgnoreCase);
                        var styleMatch = Regex.Match(responseText, @"style[""']?\s*:\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                        var genderMatch = Regex.Match(responseText, @"gender_suitability[""']?\s*:\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);

                        var fallbackResult = new
                        {
                            aesthetic_score = aestheticMatch.Success ? float.Parse(aestheticMatch.Groups[1].Value) : 0f,
                            style = styleMatch.Success ? styleMatch.Groups[1].Value : "N/A",
                            gender_suitability = genderMatch.Success ? genderMatch.Groups[1].Value : "N/A"
                        };

                        response.SetSuccessResponse(data: fallbackResult, message: "Phân tích hình ảnh thử đồ hoàn tất (fallback parsing).");
                    }
                }
                else
                {
                    response.SetErrorResponse("Không nhận được phản hồi từ dịch vụ AI.");
                }
            }
            catch (Exception ex)
            {
                response.SetErrorResponse($"Lỗi khi phân tích hình ảnh thử đồ: {ex.Message}");
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