using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.ViewHistory
{
    public class ViewHistoryRepository : IViewHistoryRepository
    {
        private readonly EcommerceShopContext _context;
        public ViewHistoryRepository(EcommerceShopContext _context)
        {
            this._context = _context;
        }
        public async Task AddOrUpdateAsync(int maKh, int? maSanPham, int? maCombo)
        {
            var lichSu = await _context.LichSuXems
            .FirstOrDefaultAsync(x => x.MaKh == maKh && (maSanPham.HasValue ? x.MaSp == maSanPham : x.MaCombo == maCombo ));

            if (lichSu != null)
            {
                // Cập nhật thời gian
                lichSu.ThoiGianXem = DateTime.UtcNow;
            }
            else
            {
                if (maSanPham.HasValue)
                {
                    // Tạo mới
                    lichSu = new LichSuXem
                    {
                        MaKh = maKh,
                        MaSp = maSanPham,
                        MaCombo = null,
                        ThoiGianXem = DateTime.Now
                    };
                }
                if (maCombo.HasValue)
                {
                    // Tạo mới
                    lichSu = new LichSuXem
                    {
                        MaKh = maKh,
                        MaSp = null,
                        MaCombo = maCombo,
                        ThoiGianXem = DateTime.Now
                    };
                }
                await _context.LichSuXems.AddAsync(lichSu);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<LichSuXem>> getHistoryAsync(int maKh, int soLuong = 10)
        {
            var data = await _context.LichSuXems
            .Where(x => x.MaKh == maKh)
            .OrderByDescending(x => x.ThoiGianXem)
            .Include(x => x.MaSpNavigation)
            .Take(soLuong)
            .Select(p => new LichSuXem
            {
                Id = p.Id,
                MaCombo = p.MaCombo,
                MaSp = p.MaSp,
                MaKh = p.MaKh,
                ThoiGianXem = p.ThoiGianXem,
            })
            .ToListAsync();
            return data;
        }
    }
}
