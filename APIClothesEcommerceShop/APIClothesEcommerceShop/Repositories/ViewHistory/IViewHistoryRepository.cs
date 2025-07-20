using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.ViewHistory
{
    public interface IViewHistoryRepository
    {
        Task AddOrUpdateAsync(int maKh, int? maSanPham, int? maCombo);
        Task<List<LichSuXem>> getHistoryAsync(int maKh, int soLuong = 10);
    }
}
