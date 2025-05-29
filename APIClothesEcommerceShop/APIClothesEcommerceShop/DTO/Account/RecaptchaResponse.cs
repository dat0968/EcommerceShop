using Newtonsoft.Json;

namespace APIClothesEcommerceShop.DTO.Account
{
    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public string Challenge_ts { get; set; }
        public string Hostname { get; set; }
        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
