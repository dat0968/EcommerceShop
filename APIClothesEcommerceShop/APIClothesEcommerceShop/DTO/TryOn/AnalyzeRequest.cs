using System.Collections.Generic;

namespace APIClothesEcommerceShop.DTO.TryOn
{
    public class AnalyzeRequest
    {
        public string ResultImageUrl { get; set; }
        public List<ProductData> ProductsData { get; set; }
    }
    public class ProductData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ComboName { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public VariantData Variant { get; set; }
        public List<ProductData> Products { get; set; } // For combos
    }

    public class VariantData
    {
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }
    }
}
