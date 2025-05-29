using APIClothesEcommerceShop.DTO;
using System.Text.Json.Serialization;

namespace APIClothesEcommerceShop.DTO
{
    public class PersonalInformationDTO
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string? SDT { get; set; }
        [JsonIgnore]
        public string? Hinh { get; set; }
        public string? VaiTro { get; set; }
        public string RefreshToken { get; set; }
    }
}
