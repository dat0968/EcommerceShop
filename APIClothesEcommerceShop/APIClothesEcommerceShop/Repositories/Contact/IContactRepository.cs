using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Repositories.Contact
{
    public interface IContactRepository
    {
        Task<IActionResult> SendContact(ContactRequestDTO request, HttpContext httpContext);
    }
}