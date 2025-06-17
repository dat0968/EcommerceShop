namespace APIClothesEcommerceShop.DTO.Shop
{
    public class ShopItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } 
        public string Image { get; set; }
        public string? PriceRange { get; set; } 
        public float? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
    public class ShopResponseDto
    {
        public List<ShopItemDTO> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
