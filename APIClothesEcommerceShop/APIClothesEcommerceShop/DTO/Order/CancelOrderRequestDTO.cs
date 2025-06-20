namespace APIClothesEcommerceShop.DTO.Order
{
    public class CancelOrderRequestDTO
    {
        public int Id { get; set; }
        public string SelectedCancelStatus { get; set; }
        public string? ReasonCancel { get; set; }
    }
}
