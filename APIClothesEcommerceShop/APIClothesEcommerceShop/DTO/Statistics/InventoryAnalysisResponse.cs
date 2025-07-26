namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class InventoryAnalysisResponse
    {
        public List<LowStockProduct> LowStockProducts { get; set; }
    }

    public class LowStockProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
    }
}