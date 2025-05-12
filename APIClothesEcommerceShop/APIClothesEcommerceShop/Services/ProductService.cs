using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.CategoryDetails;
using APIClothesEcommerceShop.Repositories.ImageProduct;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.ProductDetails;

namespace APIClothesEcommerceShop.Services
{
    public class ProductService
    {
        private readonly IImageProductRepository imageProductRepository;
        private readonly ICategoryDetailsRepository categoryDetailsRepository;
        private readonly IProductDetailsRepository productDetailsRepository;
        private readonly IProductRepository productRepository;
        private readonly EcommerceShopContext db;
        public ProductService(EcommerceShopContext db, IImageProductRepository imageProductRepository, ICategoryDetailsRepository categoryDetailsRepository, IProductDetailsRepository productDetailsRepository, IProductRepository productRepository)
        {
            this.imageProductRepository = imageProductRepository;
            this.categoryDetailsRepository = categoryDetailsRepository;
            this.productDetailsRepository = productDetailsRepository;
            this.productRepository = productRepository;
            this.db = db;
        }
        public async Task<bool> AddProduct(ProductResquestDTO model)
        {
            try
            {
                await db.Database.BeginTransactionAsync();

                // Thêm sản phẩm
                var NewProduct = new Sanpham
                {
                    TenSanPham = model.TenSanPham,
                    IsActive = true,
                };
                NewProduct = await productRepository.Add(NewProduct);

                // Thêm chi tiết sản phẩm
                foreach(var productdetail in model.ProductDetails)
                {
                    var NewProductDetail = new Chitietsanpham
                    {
                        MaSp = NewProduct.MaSp,
                        KichThuoc = productdetail.KichThuoc,
                        MauSac = productdetail.MauSac,
                        SoLuongTon = productdetail.SoLuongTon,
                        DonGia = productdetail.DonGia,
                        IsActive = true
                    };
                    NewProductDetail = await productDetailsRepository.Add(NewProductDetail);
                }

                // Thêm chi tiết danh mục
                foreach(var categorydetail in model.CategoryDetails)
                {
                    var NewCategoryDetail = new Chitietdanhmuc
                    {
                        MaDanhMucCha = categorydetail.MaDanhMucCha,
                        MaDanhMucCon = categorydetail.MaDanhMucCon,
                        MaSp = NewProduct.MaSp
                    };
                }
                return true;
                
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
