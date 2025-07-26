namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class CategoryStatisticsResponse
    {
        public int TotalCategories { get; set; }
        public List<TopCategory> TopCategories { get; set; }
    }

    public class TopCategory
    {
        public string CategoryName { get; set; }
        public int ProductsSoldCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}