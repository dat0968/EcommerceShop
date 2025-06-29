namespace APIClothesEcommerceShop.DTO.Order
{
    public class UpdateOrderDTO
    {
        public string Status { get; set; }
        public int MaNv { get; set; }
        public string PaymentMethod { get; set; }
        public string? ReasonCancel { get; set; }
    }
}
