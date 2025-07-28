namespace APIClothesEcommerceShop.DTO.TryOn
{
    public class TryOnRequest
    {
        public string ModelImage { get; set; }
        public List<string> ProductImages { get; set; }
        public List<ProductInfo> ProductsData { get; set; }
    }

    public class ProductInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public string Info { get; set; }
    }

    public class TryOnResponse
    {
        public string Image { get; set; }
        public double Score { get; set; }
        public string Style { get; set; }
        public string GenderSuitability { get; set; }
    }
}
