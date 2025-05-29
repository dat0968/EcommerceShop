namespace APIClothesEcommerceShop.DTO
{
    public class RegisterDTO
    {
        public string HoTen { get; set; }

        public string Email { get; set; }

        public string? TenTaiKhoan { get; set; }

        public string MatKhau { get; set; }
        public string RecaptchaToken { get; set; }

    }
}
