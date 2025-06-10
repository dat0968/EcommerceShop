using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.DTO;
using Microsoft.EntityFrameworkCore;
using APIClothesEcommerceShop.DTO.Combos;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Data;

namespace APIClothesEcommerceShop.Repositories.Combos
{
    public class ComboRepository : IComboRepository
    {
        private readonly EcommerceShopContext _context;

        public ComboRepository(EcommerceShopContext context)
        {
            _context = context;
        }

        public async Task<List<ComboResponseDTO>> GetAll(string? search)
        {
            try
            {
                var query = _context.Combos
                    .AsNoTracking()
                    .Where(p => p.IsActive == true)
                    .Include(c => c.Chitietcombos)
                        .ThenInclude(cc => cc.MaComboNavigation)  // Giữ nguyên navigation property
                    .Include(c => c.Chitietcombos)
                        .ThenInclude(cc => cc.MaSpNavigation)
                    .Select(p => new ComboResponseDTO
                    {
                        MaCombo = p.MaCombo,
                        TenCombo = p.TenCombo,
                        Hinh = p.Hinh,
                        PhanTramGiam = p.PhanTramGiam,
                        SoTienGiam = p.SoTienGiam,
                        SoLuong = p.SoLuong,
                        NgayBatDau = p.NgayBatDau,
                        NgayKetThuc = p.NgayKetThuc,
                        MoTa = p.MoTa,
                        IsActive = p.IsActive,
                        Chitietcombos = p.Chitietcombos.Select(cc => new DetaisComboResponseDTO
                        {
                            MaSp = cc.MaSp,
                            TenSp = cc.MaSpNavigation != null ? cc.MaSpNavigation.TenSanPham : "N/A",  // Thêm null check
                            SoLuongSp = cc.SoLuongSP
                        }).ToList()
                    });

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        p.MaCombo.ToString().Contains(search) ||
                        p.TenCombo.Contains(search));
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Nên log lỗi ở đây
                throw new Exception("Lỗi khi lấy danh sách combo", ex);
            }
        }


        public async Task<APIClothesEcommerceShop.Models.Combo?> GetById(int id)
        {
            var getCombobyID = await _context.Combos.AsNoTracking().FirstOrDefaultAsync(p => p.MaCombo == id);
            return getCombobyID;
        }

        public async Task<APIClothesEcommerceShop.Models.Combo> AddCombo(APIClothesEcommerceShop.Models.Combo newCombo)
        {
            try
            {
                _context.Combos.Add(newCombo);
                await _context.SaveChangesAsync();
                return newCombo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task EditCombo(APIClothesEcommerceShop.Models.Combo model)
        {
            try
            {
                _context.Combos.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task CancelCombo(int id)
        {
            try
            {
                var findCombo = await _context.Combos.FindAsync(id);
                if (findCombo != null)
                {
                    findCombo.IsActive = false;
                    _context.Combos.Update(findCombo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("This combo not found");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
