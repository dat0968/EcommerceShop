using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.CategoryDetails;
using APIClothesEcommerceShop.Repositories.ImageProduct;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.ProductDetails;
using Mscc.GenerativeAI;

namespace APIClothesEcommerceShop.Services
{
    public class ProductService
    {
        private readonly IImageProductRepository imageProductRepository;
        private readonly ICategoryDetailsRepository categoryDetailsRepository;
        private readonly IProductDetailsRepository productDetailsRepository;
        private readonly IProductRepository productRepository;
        private readonly EcommerceShopContext db;
        private readonly IConfiguration configuration;
        public ProductService(EcommerceShopContext db, IConfiguration configuration, IImageProductRepository imageProductRepository, ICategoryDetailsRepository categoryDetailsRepository, IProductDetailsRepository productDetailsRepository, IProductRepository productRepository)
        {
            this.imageProductRepository = imageProductRepository;
            this.categoryDetailsRepository = categoryDetailsRepository;
            this.productDetailsRepository = productDetailsRepository;
            this.productRepository = productRepository;
            this.db = db;
            this.configuration = configuration;
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

                    // Thêm hình ảnh
                    if (productdetail.Images != null)
                    {   
                        foreach (var image in productdetail.Images)
                        {
                            var NewImage = new Hinhanh
                            {
                                MaCtsp = NewProductDetail.MaCtsp,
                                TenHinhAnh = image.TenHinhAnh,
                            };
                            await imageProductRepository.Add(NewImage);
                        }
                    }                 
                }
                var Description = await GenerateDescription(NewProduct);
                NewProduct.MoTa = Description;
                await productRepository.Update(NewProduct);
                // Thêm chi tiết danh mục
                foreach (var categorydetail in model.CategoryDetails)
                {
                    var NewCategoryDetail = new Chitietdanhmuc
                    {
                        MaDanhMucCha = categorydetail.MaDanhMucCha,
                        MaDanhMucCon = categorydetail.MaDanhMucCon,
                        MaSp = NewProduct.MaSp
                    };
                    await categoryDetailsRepository.Add(NewCategoryDetail);
                }

                await db.Database.CommitTransactionAsync();
                return true;
                
            }catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception("Error", ex);
            }
        }

        public async Task<bool> UpdateProduct(int id, ProductResquestDTO model)
        {
            try
            {
                await db.Database.BeginTransactionAsync();

                // Cập nhật sản phẩm
                var UpdateProduct = new Sanpham
                {
                    MaSp = id,
                    TenSanPham = model.TenSanPham,
                    MoTa = model.MoTa,
                    IsActive = true,
                };
                await productRepository.Update(UpdateProduct);

                // Xóa chi tiết sản phẩm cần xóa
                var FindProductDetails = await productDetailsRepository.GetDetailProductByProductId(id);
                var FindProductDetailsId = FindProductDetails.Select(p => p.MaCtsp).ToList();
                var GetProductDetailsId = model.ProductDetails.Select(p => p.MaCtsp).ToList();

                var FindDetailToDelete = FindProductDetails.Where(p => GetProductDetailsId.Contains(p.MaCtsp) == false);
                foreach (var detail in FindDetailToDelete)
                {
                    await productDetailsRepository.Cancel(detail.MaCtsp);
                }

                // Cập nhật hoặc thêm mới chi tiết sản phẩm
                foreach(var detail in model.ProductDetails)
                {
                    if (FindProductDetailsId.Contains(detail.MaCtsp.Value))
                    {
                        var UpdateProductDetails = new Chitietsanpham
                        {
                            MaCtsp = detail.MaCtsp.Value,
                            KichThuoc = detail.KichThuoc,
                            MauSac = detail.MauSac,
                            IsActive = true,
                            SoLuongTon = detail.SoLuongTon,
                            DonGia = detail.DonGia,
                        };
                        await productDetailsRepository.Update(UpdateProductDetails);

                        // Xử lí hình ảnh sản phẩm
                        await imageProductRepository.DeleteImageProductByMaCtSp(detail.MaCtsp.Value);
                        if(detail.Images != null)
                        {
                            foreach(var image in detail.Images)
                            {
                                var NewImage = new Hinhanh
                                {
                                    MaCtsp = detail.MaCtsp.Value,
                                    TenHinhAnh = image.TenHinhAnh,
                                };
                                await imageProductRepository.Add(NewImage);
                            }
                        }
                    }
                    else
                    {
                        var NewProductDetails = new Chitietsanpham
                        {
                            MaSp = id,
                            KichThuoc = detail.KichThuoc,
                            MauSac = detail.MauSac,
                            IsActive = true,
                            SoLuongTon = detail.SoLuongTon,
                            DonGia = detail.DonGia,
                        };
                        NewProductDetails = await productDetailsRepository.Add(NewProductDetails);
                        if(detail.Images != null)
                        {
                            foreach (var image in detail.Images)
                            {
                                var NewImage = new Hinhanh
                                {
                                    MaCtsp = detail.MaCtsp.Value,
                                    TenHinhAnh = image.TenHinhAnh,
                                };
                                await imageProductRepository.Add(NewImage);
                            }
                        }
                    }
                }

                // Xóa đi những chi tiết danh mục cần xóa
                var findDetailCategory = await categoryDetailsRepository.GetDetailCategoryByProductId(id);
                var findDetailCategoryId = findDetailCategory.Select(p => new CategoryDetailsResponseDTO
                {
                    MaDanhMucCha = p.MaDanhMucCha,
                    MaDanhMucCon = p.MaDanhMucCon,
                });
                var RequestCategoryDetailId = model.CategoryDetails.Select(p => new CategoryDetailsResponseDTO
                {
                    MaDanhMucCha = p.MaDanhMucCha,
                    MaDanhMucCon = p.MaDanhMucCon
                });

                var CategoryDetailsToDelete = findDetailCategory.Where(p => !RequestCategoryDetailId.Any(x => x.MaDanhMucCha == p.MaDanhMucCha && x.MaDanhMucCon == p.MaDanhMucCon));
                foreach(var category in CategoryDetailsToDelete)
                {
                    await categoryDetailsRepository.Delete(category.MaDanhMucCha, category.MaDanhMucCon, id);
                }

                // Cập nhật hoặc thêm chi tiết danh mục
                foreach(var category in model.CategoryDetails)
                {
                    if(findDetailCategoryId.Any(x => x.MaDanhMucCha == category.MaDanhMucCha && x.MaDanhMucCon == category.MaDanhMucCon))
                    {
                        var UpdateCategoryDetails = new Chitietdanhmuc
                        {
                            MaSp = id,
                            MaDanhMucCha = category.MaDanhMucCha,
                            MaDanhMucCon = category.MaDanhMucCon,
                        };
                        await categoryDetailsRepository.Update(UpdateCategoryDetails);
                    }
                    else
                    {
                        var NewCategoryDetail = new Chitietdanhmuc
                        {
                            MaSp = id,
                            MaDanhMucCon = category.MaDanhMucCon,
                            MaDanhMucCha = category.MaDanhMucCha
                        };
                        await categoryDetailsRepository.Add(NewCategoryDetail);
                    }
                }
                await db.Database.CommitTransactionAsync();
                return true;

            }
            catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception("Error", ex);
            }   
        }


        public async Task<string> GenerateDescription(Sanpham ProductInformation)
        {
            var mykey = configuration["Chatbot:Gemini:GeminiAPIKey"];
            var productname = ProductInformation.TenSanPham;
            var colors = ProductInformation.Chitietsanphams.Select(x => x.MauSac);
            var sizes = ProductInformation.Chitietsanphams.Select(x => x.KichThuoc);
            var variants =
                $"Màu sắc có sẵn: {string.Join(", ", colors)}.\n" + 
                $"Kích thước: {string.Join(", ", sizes)}.\n";
            var commitment = "CAM KẾT - ĐẢM BẢO:" +
                "\r\n\r\n- Đảm bảo vải chuẩn cotton chất lượng cao." +
                "\r\n\r\n- Hàng có sẵn, giao hàng ngay khi nhận được đơn đặt hàng ." +
                "\r\n\r\n- Hoàn tiền 100% nếu sản phẩm lỗi, nhầm hoặc không giống với mô tả." +
                "\r\n\r\n- Chấp nhận đổi hàng khi size không vừa (vui lòng nhắn tin riêng cho shop)." +
                "\r\n\r\n- Giao hàng toàn quốc." +
                "\r\n\r\n" +
                "\r\n\r\nĐIỀU KIỆN ĐỔI TRẢ:" +
                "\r\n\r\n- Hỗ trợ trong vòng 03 ngày từ khi nhận hàng." +
                "\r\n\r\n- Hàng hoá vẫn còn mới nguyên tem mác, chưa qua sử dụng." +
                "\r\n\r\n- Hàng hoá bị lỗi hoặc hư hỏng do vận chuyển hoặc do nhà sản xuất.";

            var prompt = $"Viết mô tả sản phẩm hấp dẫn cho \"{productname}\".\n" +
                 $"Thông tin chi tiết:\n{variants}\n\n" +
                 "Thông tin cam kết và chính sách đổi trả:\n" +
                 $"{commitment}\n\n" +
                 "Hãy viết nội dung mô tả tự nhiên, thu hút khách hàng, nhấn mạnh ưu điểm của sản phẩm, chất liệu, kiểu dáng và chính sách của cửa hàng.";

            var googleAI = new GoogleAI(apiKey: mykey);
            var model = googleAI.GenerativeModel(model: Model.Gemini15Flash);

            var response = await model.GenerateContent(prompt);
            return response.Text;
        }
    }
}
