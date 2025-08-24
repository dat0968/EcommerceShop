
using APIClothesEcommerceShop.DTO.VNPAY;

namespace APIClothesEcommerceShop.Repositories.VNPAY
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}