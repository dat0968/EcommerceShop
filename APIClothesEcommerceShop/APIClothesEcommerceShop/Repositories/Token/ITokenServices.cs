using APIClothesEcommerceShop.DTO;

namespace APIClothesEcommerceShop.Repositories.Token
{
    public interface ITokenServices
    {
        public string GenerateAccessToken(PersonalInformationDTO model);
        public string GenerateRefreshToken();

    }
}
