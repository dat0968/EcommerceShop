using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Order
{
    public interface IOrderRepository
    {
        Task<List<OrderResponseDTO>> GetAll(string? search, string? filter);
        Task<Hoadon> GetbyId(int id);
        Task<Hoadon> CreateOrder(Hoadon model);
        Task UpdateStatusOrders(int id, string status, int? MaNv, string paymentmethod, string? reasonCancel);
        Task CancelOrders(int id, string selectedCancelStatus, string? ReasonCancel);
    }
}
