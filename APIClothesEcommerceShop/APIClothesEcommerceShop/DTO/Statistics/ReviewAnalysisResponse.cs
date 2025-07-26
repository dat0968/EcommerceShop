namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class ReviewAnalysisResponse
    {
        public double AverageRating { get; set; }
        public ProductReviewSummary? MostReviewedProduct { get; set; }
        public ProductReviewSummary? HighestRatedProduct { get; set; }
        public ProductReviewSummary? LowestRatedProduct { get; set; }
    }

    public class ProductReviewSummary
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}