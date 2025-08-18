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
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.DTO.ImageProduct;

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
                    .Where(p => p.IsActive == true )
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
                        ReviewCount = p.Chitietcombos.SelectMany(ct => ct.MaSpNavigation.DanhGias).Count(),
                        AverageRating = p.Chitietcombos.SelectMany(ct => ct.MaSpNavigation.DanhGias).Any() 
                                      ? p.Chitietcombos.SelectMany(ct => ct.MaSpNavigation.DanhGias).Average(dg => dg.Rating) 
                                      : 5,
                        Chitietcombos = p.Chitietcombos.Select(cc => new DetaisComboResponseDTO
                        {
                            MaSp = cc.MaSp,
                            TenSp = cc.MaSpNavigation != null ? cc.MaSpNavigation.TenSanPham : "N/A",
                            SoLuongSp = cc.SoLuongSP,
                            SanPhamCTs = cc.MaSpNavigation!.Chitietsanphams
                            .Where(ctsp => ctsp.IsActive == true)
                            .Select(ctsp => new ProductDetailResponseDTO
                            {
                                MaCtsp = ctsp.MaCtsp,
                                KichThuoc = ctsp.KichThuoc,
                                MauSac = ctsp.MauSac,
                                SoLuongTon = ctsp.SoLuongTon,
                                DonGia = ctsp.DonGia,
                                Images = ctsp.Hinhanhs.Select(img => new ImageProductResponseDTO
                                {
                                    MaCtsp = img.MaCtsp,
                                    TenHinhAnh = img.TenHinhAnh
                                }).ToList()
                            }).ToList()
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


        public async Task<ComboResponseDTO?> GetById(int id)
        {
            var getCombobyID = await _context.Combos.AsNoTracking()
                .Include(p => p.Chitietcombos)
                .ThenInclude(p => p.MaSpNavigation)
                .ThenInclude(p => p.DanhGias) // Include reviews for products in combo
                .Include(p => p.Chitietcombos)
                .ThenInclude(p => p.MaSpNavigation)
                .ThenInclude(p => p.Chitietsanphams)
                .FirstOrDefaultAsync(p => p.MaCombo == id);
            if (getCombobyID == null)
            {
                throw new Exception("Not Found Combo");
            }

            var allReviews = getCombobyID.Chitietcombos.SelectMany(ct => ct.MaSpNavigation.DanhGias).ToList();

            var ResponseCombo = new ComboResponseDTO
            {
                MaCombo = getCombobyID.MaCombo,
                TenCombo = getCombobyID.TenCombo,
                Hinh = getCombobyID.Hinh,
                SoLuong = getCombobyID.SoLuong,
                MoTa = getCombobyID.MoTa,
                NgayBatDau = getCombobyID.NgayBatDau,
                NgayKetThuc = getCombobyID.NgayKetThuc,
                PhanTramGiam = getCombobyID.PhanTramGiam,
                SoTienGiam = getCombobyID.SoTienGiam,
                IsActive = getCombobyID.IsActive,
                ReviewCount = allReviews.Count(),
                AverageRating = allReviews.Any() ? allReviews.Average(dg => dg.Rating) : 5,
                Chitietcombos = getCombobyID.Chitietcombos.Select(p => new DetaisComboResponseDTO
                {
                    MaSp = p.MaSp,
                    TenSp = p.MaSpNavigation.TenSanPham,
                    SanPhamCTs = p.MaSpNavigation.Chitietsanphams.Where(details => details.IsActive == true).Select(details => new ProductDetailResponseDTO
                    {
                        MaCtsp = details.MaCtsp,
                        KichThuoc = details.KichThuoc,
                        MauSac = details.MauSac,
                        SoLuongTon = details.SoLuongTon,
                        DonGia = details.DonGia,
                    }).ToList(),
                    SoLuongSp = p.SoLuongSP,
                }).ToList(),
            };
            return ResponseCombo;
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
