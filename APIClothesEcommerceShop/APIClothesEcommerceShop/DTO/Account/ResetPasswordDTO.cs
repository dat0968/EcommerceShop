namespace APIClothesEcommerceShop.DTO
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public bool LoginAfterReset { get; set; }
    }
}