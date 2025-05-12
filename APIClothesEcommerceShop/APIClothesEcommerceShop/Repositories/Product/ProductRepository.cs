using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceShopContext db;
        public ProductRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public Task<ProductResponseDTO> Add(AddProductResquestDTO model)
        {
            throw new NotImplementedException();
        }

        public async Task Cancel(int id)
        {
            try
            {
                var findProduct = await db.Sanphams.FirstOrDefaultAsync(p => p.MaSp == id);
                if (findProduct == null)
                {
                    throw new Exception("Not Found Product");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<ProductResponseDTO> GetAll()
        {
            //try
            //{
            //    var GetProduct = await db.Sanphams.AsNoTracking().Select(p => new ProductResponseDTO
            //    {
            //        MaSp = p.MaSp,
            //        TenSanPham = p.TenSanPham,
            //        MoTa = p.MoTa,
            //        CategoryDetails = db.Chitietdanhmucs.Select(p => new CategoryDetailsResponseDTO
            //        {
            //            MaDanhMucCha = p.MaDanhMucCha,
            //            MaDanhMucCon = p.MaDanhMucCon
            //        }).ToList(),
            //        ProductDetails = db.Chitietsanphams.Select(p => new ProductDetailResponseDTO
            //        {
            //            MaCtsp = p.MaCtsp,
            //            KichThuoc = p.KichThuoc,
            //            MauSac = p.MauSac,
            //            SoLuongTon = p.SoLuongTon,
            //            DonGia = p.DonGia,
            //            Hinhanhs = db.Hinhanhs.
            //        }).Where(p => p.IsActive == false).ToList(),
            //    }).Where(p => p.IsActive == false).ToListAsync();

            //}
            throw new NotImplementedException();
        }

        public Task<ProductResponseDTO> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ProductResponseDTO> Update(UpdateProductResquestDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
