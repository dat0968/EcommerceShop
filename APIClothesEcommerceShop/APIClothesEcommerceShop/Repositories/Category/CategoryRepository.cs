using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Category;
using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using APIClothesEcommerceShop.DTO.Category.CategoryChild;
using APIClothesEcommerceShop.DTO.Category.CategoryDetail;
using APIClothesEcommerceShop.DTO.Category.CategoryParent;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public class CategoryRepository(EcommerceShopContext db) : Repository<Danhmuccha>(db), ICategoryRepository
    {
        //private readonly EcommerceShopContext db;
        //public CategoryRepository(EcommerceShopContext db)
        //{
        //    this.db = db;
        //}
        public async Task<List<CategoryResponseDTO>> GetAllSmallCategories()
        {
            var GetSmallCategories = await db.Danhmuccons.ToListAsync();


            var result = GetSmallCategories.Select(d => new CategoryResponseDTO
            {
                MaDanhMucCon = d.MaDanhMucCon,
                TenDanhMucCon = d.TenDanhMucCon,
            }).ToList();
            return result;
        }
        public async Task<List<CategoryResponseDTO>> GetAllBigCategories()
        {
            var GetBigCategories = await db.Danhmucchas
                .Include(p => p.Chitietdanhmucs)
                .ThenInclude(p => p.MaDanhMucConNavigation)
                .AsNoTracking()
                .ToListAsync();


            var result = GetBigCategories.Select(d => new CategoryResponseDTO
            {
                MaDanhMucCha = d.MaDanhMucCha,
                TenDanhMucCha = d.TenDanhMucCha,
                Chitietdanhmucs = d.Chitietdanhmucs
                .GroupBy(ct => ct.MaDanhMucCon)
                .Select(g => g.First())
                .Select(ct => new CategoryDetailsResponseDTO
                {
                    MaDanhMucCha = ct.MaDanhMucCha,
                    MaDanhMucCon = ct.MaDanhMucCon,
                    TenDanhMucCon = ct.MaDanhMucConNavigation.TenDanhMucCon,
                }).ToList()
            }).ToList();
            return result;
        }
        public async Task<ResponseAPI<List<CategoryResponseDTO>>> GetAllCategoriesAsync()
        {
            ResponseAPI<List<CategoryResponseDTO>> response = new();
            try
            {
                var dataMain = await db.Chitietdanhmucs
                    .Include(x => x.MaDanhMucChaNavigation)
                    .Include(x => x.MaDanhMucConNavigation)
                    .Include(x => x.MaSpNavigation)
                        .ThenInclude(sp => sp.Chitietsanphams)
                            .ThenInclude(ctsp => ctsp.Hinhanhs)
                    .AsNoTracking()
                    .ToListAsync();

                var result = dataMain.Select(x => new CategoryResponseDTO
                {
                    MaDanhMucCha = x.MaDanhMucCha,
                    TenDanhMucCha = x.MaDanhMucChaNavigation.TenDanhMucCha,
                    MaDanhMucCon = x.MaDanhMucCon,
                    TenDanhMucCon = x.MaDanhMucConNavigation.TenDanhMucCon,
                    MaSp = x.MaSp,
                    TenSanPham = x.MaSpNavigation.TenSanPham,
                    MoTa = x.MaSpNavigation.MoTa,
                    DetailProducts = x.MaSpNavigation.Chitietsanphams.Select(ctsp => new DtProduct
                    {
                        MaCtsp = ctsp.MaCtsp,
                        KichThuoc = ctsp.KichThuoc,
                        MauSac = ctsp.MauSac,
                        SoLuongTon = ctsp.SoLuongTon,
                        DonGia = ctsp.DonGia,
                        IsActive = ctsp.IsActive,
                        ImageUrl = ctsp.Hinhanhs.FirstOrDefault()?.TenHinhAnh ?? string.Empty
                    }).ToList(),
                    IsActiveDanhMucCha = x.MaDanhMucChaNavigation.IsActive,
                    IsActiveDanhMucCon = x.MaDanhMucConNavigation.IsActive
                }).ToList();

                response.SetSuccessResponse(data: result);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region [Danh mục cha]
        // Danh sách dữ liệu danh mục cha
        public async Task<ResponseAPI<List<CategoryParentResponseDTO>>> GetCategoryParentAsync()
        {
            ResponseAPI<List<CategoryParentResponseDTO>> response = new();
            try
            {
                var category = await db.Danhmucchas.AsNoTracking().ToListAsync();
                if (category == null || category.Count == 0)
                    throw new Exception("Dữ liệu danh mục cha không tìm thấy trong hệ thống.");
                else
                {
                    response.SetSuccessResponse(data: category.Select(x => x.ToCategoryParentResponseDTO()).ToList());
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public Task<List<Danhmuccha>> GetCategories()
        {
            throw new NotImplementedException();
        }
        // Xem chi tiết một danh mục cha
        public async Task<ResponseAPI<CategoryParentResponseDTO>> GetCategoryByIdAsync(int id)
        {
            ResponseAPI<CategoryParentResponseDTO> response = new();
            try
            {
                var category = await db.Danhmucchas
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == id);
                if (category == null)
                    response.SetErrorResponse("Không tìm thấy danh mục cha.");
                else
                {
                    var dto = new CategoryParentResponseDTO
                    {
                        MaDanhMucCha = category.MaDanhMucCha,
                        TenDanhMucCha = category.TenDanhMucCha,
                        IsActive = category.IsActive
                    };
                    response.SetSuccessResponse(data: dto);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        // Thêm mới hoặc cập nhật danh mục cha (Upsert)
        public async Task<ResponseAPI<CategoryParentResponseDTO>> UpsertCategoryAsync(int id, CategoryParentRequestDTO categoryDto)
        {
            ResponseAPI<CategoryParentResponseDTO> response = new();
            try
            {
                var existingCategory = await db.Danhmucchas.FindAsync(id);
                if (existingCategory == null)
                {
                    var newCategory = new Danhmuccha
                    {
                        TenDanhMucCha = categoryDto.TenDanhMucCha,
                        IsActive = categoryDto.IsActive
                    };
                    db.Danhmucchas.Add(newCategory);
                    await db.SaveChangesAsync();
                    var dto = new CategoryParentResponseDTO
                    {
                        MaDanhMucCha = newCategory.MaDanhMucCha,
                        TenDanhMucCha = newCategory.TenDanhMucCha,
                        IsActive = newCategory.IsActive
                    };
                    response.SetSuccessResponse(data: dto, message: "Thêm danh mục cha thành công.");
                }
                else
                {
                    existingCategory.TenDanhMucCha = categoryDto.TenDanhMucCha;
                    existingCategory.IsActive = categoryDto.IsActive;
                    await db.SaveChangesAsync();
                    var dto = new CategoryParentResponseDTO
                    {
                        MaDanhMucCha = existingCategory.MaDanhMucCha,
                        TenDanhMucCha = existingCategory.TenDanhMucCha,
                        IsActive = existingCategory.IsActive
                    };
                    response.SetSuccessResponse(data: dto, message: "Cập nhật danh mục cha thành công.");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        // Xóa danh mục cha
        public async Task<ResponseAPI<dynamic>> DeleteCategoryAsync(int id)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                var category = await db.Danhmucchas.FindAsync(id);
                if (category == null)
                {
                    response.SetErrorResponse("Không tìm thấy danh mục cha.");
                    return response;
                }
                if (db.Chitietdanhmucs.Any(x => x.MaDanhMucCha == category.MaDanhMucCha))
                {
                    throw new Exception("Không thể xóa danh mục cha vì có chi tiết danh mục liên quan");
                }
                db.Danhmucchas.Remove(category);
                await db.SaveChangesAsync();
                response.SetSuccessResponse(data: true, message: "Xóa danh mục cha thành công.");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
        #endregion

        #region [Danh mục con]
        public async Task<ResponseAPI<List<CategoryChildResponseDTO>>> GetAllSubCategoriesAsync()
        {
            ResponseAPI<List<CategoryChildResponseDTO>> response = new();
            try
            {
                var data = await db.Danhmuccons
                    .AsNoTracking()
                    .ToListAsync();

                var result = data.Select(x => new CategoryChildResponseDTO
                {
                    MaDanhMucCon = x.MaDanhMucCon,
                    TenDanhMucCon = x.TenDanhMucCon,
                    IsActive = x.IsActive
                }).ToList();

                response.SetSuccessResponse(data: result);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<CategoryChildResponseDTO>> GetSubCategoryByIdAsync(int id)
        {
            ResponseAPI<CategoryChildResponseDTO> response = new();
            try
            {
                var subCategory = await db.Danhmuccons
                    .FirstOrDefaultAsync(x => x.MaDanhMucCon == id);
                if (subCategory == null)
                    response.SetErrorResponse("Không tìm thấy danh mục con.");
                else
                {
                    var dto = new CategoryChildResponseDTO
                    {
                        MaDanhMucCon = subCategory.MaDanhMucCon,
                        TenDanhMucCon = subCategory.TenDanhMucCon,
                        IsActive = subCategory.IsActive
                    };
                    response.SetSuccessResponse(data: dto);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<CategoryChildResponseDTO>> UpsertSubCategoryAsync(int id, CategoryChildRequestDTO subCategoryDto)
        {
            ResponseAPI<CategoryChildResponseDTO> response = new();
            try
            {
                var existing = await db.Danhmuccons.FindAsync(id);
                if (existing == null)
                {
                    var newSubCategory = new Danhmuccon
                    {
                        TenDanhMucCon = subCategoryDto.TenDanhMucCon,
                        IsActive = subCategoryDto.IsActive
                    };
                    db.Danhmuccons.Add(newSubCategory);
                    await db.SaveChangesAsync();
                    var dto = new CategoryChildResponseDTO
                    {
                        MaDanhMucCon = newSubCategory.MaDanhMucCon,
                        TenDanhMucCon = newSubCategory.TenDanhMucCon,
                        IsActive = newSubCategory.IsActive
                    };
                    response.SetSuccessResponse(data: dto, message: "Thêm danh mục con thành công.");
                }
                else
                {
                    existing.TenDanhMucCon = subCategoryDto.TenDanhMucCon;
                    existing.IsActive = subCategoryDto.IsActive;
                    await db.SaveChangesAsync();
                    var dto = new CategoryChildResponseDTO
                    {
                        MaDanhMucCon = existing.MaDanhMucCon,
                        TenDanhMucCon = existing.TenDanhMucCon,
                        IsActive = existing.IsActive
                    };
                    response.SetSuccessResponse(data: dto, message: "Cập nhật danh mục con thành công.");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> DeleteSubCategoryAsync(int id)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                var subCategory = await db.Danhmuccons.FindAsync(id);
                if (subCategory == null)
                {
                    response.SetErrorResponse("Không tìm thấy danh mục con.");
                    return response;
                }
                if (db.Chitietdanhmucs.Any(x => x.MaDanhMucCon == subCategory.MaDanhMucCon))
                {
                    throw new Exception("Không thể xóa danh mục con vì có chi tiết danh mục liên quan");
                }
                db.Danhmuccons.Remove(subCategory);
                await db.SaveChangesAsync();
                response.SetSuccessResponse(data: true, message: "Xóa danh mục con thành công.");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
        #endregion

        #region [Chi tiết danh mục]

        public async Task<ResponseAPI<List<CategoryDetailResponseDTO>>> GetAllCategoryDetailsAsync()
        {
            ResponseAPI<List<CategoryDetailResponseDTO>> response = new();
            try
            {
                var data = await db.Chitietdanhmucs
                    .AsNoTracking()
                    .ToListAsync();

                var result = data.Select(x => new CategoryDetailResponseDTO
                {
                    MaDanhMucCha = x.MaDanhMucCha,
                    MaDanhMucCon = x.MaDanhMucCon,
                    MaSp = x.MaSp
                }).ToList();

                response.SetSuccessResponse(data: result);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<CategoryDetailResponseDTO>> GetCategoryDetailByIdAsync(int maDanhMucCha, int maDanhMucCon, int maSp)
        {
            ResponseAPI<CategoryDetailResponseDTO> response = new();
            try
            {
                var detail = await db.Chitietdanhmucs
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == maDanhMucCha && x.MaDanhMucCon == maDanhMucCon && x.MaSp == maSp);
                if (detail == null)
                    response.SetErrorResponse("Không tìm thấy chi tiết danh mục.");
                else
                {
                    var dto = new CategoryDetailResponseDTO
                    {
                        MaDanhMucCha = detail.MaDanhMucCha,
                        MaDanhMucCon = detail.MaDanhMucCon,
                        MaSp = detail.MaSp
                    };
                    response.SetSuccessResponse(data: dto);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<CategoryDetailResponseDTO>> UpsertCategoryDetailAsync(CategoryDetailRequestDTO detailDto)
        {
            ResponseAPI<CategoryDetailResponseDTO> response = new();
            try
            {
                var existing = await db.Chitietdanhmucs
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == detailDto.MaDanhMucCha && x.MaDanhMucCon == detailDto.MaDanhMucCon && x.MaSp == detailDto.MaSp);
                if (existing == null)
                {
                    var newDetail = new Chitietdanhmuc
                    {
                        MaDanhMucCha = detailDto.MaDanhMucCha,
                        MaDanhMucCon = detailDto.MaDanhMucCon,
                        MaSp = detailDto.MaSp
                    };
                    db.Chitietdanhmucs.Add(newDetail);
                    await db.SaveChangesAsync();
                    var dto = new CategoryDetailResponseDTO
                    {
                        MaDanhMucCha = newDetail.MaDanhMucCha,
                        MaDanhMucCon = newDetail.MaDanhMucCon,
                        MaSp = newDetail.MaSp
                    };
                    response.SetSuccessResponse(data: dto, message: "Thêm chi tiết danh mục thành công.");
                }
                else
                {
                    // Nếu có thêm thuộc tính thì cập nhật ở đây
                    await db.SaveChangesAsync();
                    var dto = new CategoryDetailResponseDTO
                    {
                        MaDanhMucCha = existing.MaDanhMucCha,
                        MaDanhMucCon = existing.MaDanhMucCon,
                        MaSp = existing.MaSp
                    };
                    response.SetSuccessResponse(data: dto, message: "Chi tiết danh mục đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> DeleteCategoryDetailAsync(int maDanhMucCha, int maDanhMucCon, int maSp)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                var detail = await db.Chitietdanhmucs
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == maDanhMucCha && x.MaDanhMucCon == maDanhMucCon && x.MaSp == maSp);
                if (detail == null)
                {
                    response.SetErrorResponse("Không tìm thấy chi tiết danh mục.");
                    return response;
                }
                db.Chitietdanhmucs.Remove(detail);
                await db.SaveChangesAsync();
                response.SetSuccessResponse(data: true, message: "Xóa chi tiết danh mục thành công.");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
        #endregion
    }
}