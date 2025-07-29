using System.Collections.Generic;

namespace APIClothesEcommerceShop.DTO.Gemini
{
    public class GeminiRequest
    {
        public string ImageDataUrl { get; set; }
        public List<ProductData> ProductsData { get; set; }
    }

    public class ProductData
    {
        // Define properties based on what the Vue component sends
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        // Add any other relevant properties
    }
}
