using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.Utils;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public class CategoryRepository(EcommerceShopContext db) : Repository<Danhmuccha>(db), ICategoryRepository
    {
        private readonly EcommerceShopContext _db = db;
        public async Task<ResponseAPI<List<Danhmuccha>>> GetAllCategoriesAsync()
        {
            ResponseAPI<List<Danhmuccha>> response = new();
            try
            {
                var dataMain = base.GetAllAsync(includeProperties: "Chitietdanhmucs.MaDanhMucConNavigation,Chitietdanhmucs.MaSpNavigation").Result.ToList();
                response.SetSuccessResponse(data: dataMain);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region [Danh mục cha]
        // Xem chi tiết một danh mục cha
        public async Task<ResponseAPI<Danhmuccha>> GetCategoryByIdAsync(int id)
        {
            ResponseAPI<Danhmuccha> response = new();
            try
            {
                var category = await _db.Danhmucchas
                    .Include(x => x.Chitietdanhmucs)
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == id);
                if (category == null)
                    response.SetErrorResponse("Không tìm thấy danh mục cha.");
                else
                    response.SetSuccessResponse(data: category);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        // Thêm mới hoặc cập nhật danh mục cha (Upsert)
        public async Task<ResponseAPI<Danhmuccha>> UpsertCategoryAsync(Danhmuccha category)
        {
            ResponseAPI<Danhmuccha> response = new();
            try
            {
                var existingCategory = await _db.Danhmucchas.FindAsync(category.MaDanhMucCha);
                if (existingCategory == null)
                {
                    // Thêm mới
                    _db.Danhmucchas.Add(category);
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse(data: category, message: "Thêm danh mục cha thành công.");
                }
                else
                {
                    // Cập nhật
                    existingCategory.TenDanhMucCha = category.TenDanhMucCha;
                    existingCategory.IsActive = category.IsActive;
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse(data: existingCategory, message: "Cập nhật danh mục cha thành công.");
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
                var category = await _db.Danhmucchas.FindAsync(id);
                if (category == null)
                {
                    response.SetErrorResponse("Không tìm thấy danh mục cha.");
                    return response;
                }
                if (_db.Chitietdanhmucs.Any(x => x.MaDanhMucCha == category.MaDanhMucCha))
                {
                    throw new Exception("Không thể xóa danh mục cha vì có chi tiết danh mục liên quan");
                }
                _db.Danhmucchas.Remove(category);
                await _db.SaveChangesAsync();
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
        public async Task<ResponseAPI<List<Danhmuccon>>> GetAllSubCategoriesAsync()
        {
            ResponseAPI<List<Danhmuccon>> response = new();
            try
            {
                var data = await _db.Danhmuccons
                    .Include(x => x.Chitietdanhmucs)
                    .AsNoTracking()
                    .ToListAsync();
                response.SetSuccessResponse(data: data);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<Danhmuccon>> GetSubCategoryByIdAsync(int id)
        {
            ResponseAPI<Danhmuccon> response = new();
            try
            {
                var subCategory = await _db.Danhmuccons
                    .Include(x => x.Chitietdanhmucs)
                    .FirstOrDefaultAsync(x => x.MaDanhMucCon == id);
                if (subCategory == null)
                    response.SetErrorResponse("Không tìm thấy danh mục con.");
                else
                    response.SetSuccessResponse(data: subCategory);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<Danhmuccon>> UpsertSubCategoryAsync(Danhmuccon subCategory)
        {
            ResponseAPI<Danhmuccon> response = new();
            try
            {
                var existing = await _db.Danhmuccons.FindAsync(subCategory.MaDanhMucCon);
                if (existing == null)
                {
                    _db.Danhmuccons.Add(subCategory);
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse(data: subCategory, message: "Thêm danh mục con thành công.");
                }
                else
                {
                    existing.TenDanhMucCon = subCategory.TenDanhMucCon;
                    existing.IsActive = subCategory.IsActive;
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse(data: existing, message: "Cập nhật danh mục con thành công.");
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
                var subCategory = await _db.Danhmuccons.FindAsync(id);
                if (subCategory == null)
                {
                    response.SetErrorResponse("Không tìm thấy danh mục con.");
                    return response;
                }
                if (_db.Chitietdanhmucs.Any(x => x.MaDanhMucCon == subCategory.MaDanhMucCon))
                {
                    throw new Exception("Không thể xóa danh mục con vì có chi tiết danh mục liên quan");
                }
                _db.Danhmuccons.Remove(subCategory);
                await _db.SaveChangesAsync();
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

        public async Task<ResponseAPI<List<Chitietdanhmuc>>> GetAllCategoryDetailsAsync()
        {
            ResponseAPI<List<Chitietdanhmuc>> response = new();
            try
            {
                var data = await _db.Chitietdanhmucs
                    .Include(x => x.MaDanhMucChaNavigation)
                    .Include(x => x.MaDanhMucConNavigation)
                    .Include(x => x.MaSpNavigation)
                    .AsNoTracking()
                    .ToListAsync();
                response.SetSuccessResponse(data: data);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<Chitietdanhmuc>> GetCategoryDetailByIdAsync(int maDanhMucCha, int maDanhMucCon, int maSp)
        {
            ResponseAPI<Chitietdanhmuc> response = new();
            try
            {
                var detail = await _db.Chitietdanhmucs
                    .Include(x => x.MaDanhMucChaNavigation)
                    .Include(x => x.MaDanhMucConNavigation)
                    .Include(x => x.MaSpNavigation)
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == maDanhMucCha && x.MaDanhMucCon == maDanhMucCon && x.MaSp == maSp);
                if (detail == null)
                    response.SetErrorResponse("Không tìm thấy chi tiết danh mục.");
                else
                    response.SetSuccessResponse(data: detail);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<Chitietdanhmuc>> UpsertCategoryDetailAsync(Chitietdanhmuc detail)
        {
            ResponseAPI<Chitietdanhmuc> response = new();
            try
            {
                var existing = await _db.Chitietdanhmucs
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == detail.MaDanhMucCha && x.MaDanhMucCon == detail.MaDanhMucCon && x.MaSp == detail.MaSp);
                if (existing == null)
                {
                    _db.Chitietdanhmucs.Add(detail);
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse(data: detail, message: "Thêm chi tiết danh mục thành công.");
                }
                else
                {
                    // Nếu có thêm thuộc tính thì cập nhật ở đây
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse(data: existing, message: "Chi tiết danh mục đã tồn tại.");
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
                var detail = await _db.Chitietdanhmucs
                    .FirstOrDefaultAsync(x => x.MaDanhMucCha == maDanhMucCha && x.MaDanhMucCon == maDanhMucCon && x.MaSp == maSp);
                if (detail == null)
                {
                    response.SetErrorResponse("Không tìm thấy chi tiết danh mục.");
                    return response;
                }
                _db.Chitietdanhmucs.Remove(detail);
                await _db.SaveChangesAsync();
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
