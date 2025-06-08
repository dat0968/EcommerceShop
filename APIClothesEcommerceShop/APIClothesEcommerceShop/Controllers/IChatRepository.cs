using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    internal interface IChatRepository
    {
        Task<IActionResult> GetFirebaseToken(string userId);
        Task<IActionResult> SyncUserToFirebase(string userId, string email, string fullName, string role);
        Task<IActionResult> UpdateUserStatus(string userId, bool isOnline);
    }
}