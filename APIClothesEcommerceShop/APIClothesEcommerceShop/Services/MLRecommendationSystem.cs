using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.ImageProduct;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.DTO.RecommendProduct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace APIClothesEcommerceShop.Services
{
    public class MLRecommendationSystem
    {
        private readonly MLContext mlContext;
        private readonly EcommerceShopContext db;
        private ITransformer model;
        public MLRecommendationSystem(EcommerceShopContext db)
        {
            mlContext = new MLContext();
            this.db = db;
        }
        private async Task TrainModel()
        {
            try
            {
                // Truy suất dữ liệu hóa đơn
                var cthoadons = await db.Cthoadons.AsNoTracking()
                                .Include(p => p.MaHdNavigation)
                                .Include(p => p.MaCtspNavigation)
                                .Where(p => p.MaCtsp.HasValue && p.MaHdNavigation != null && p.MaHdNavigation.MaKh.HasValue).ToListAsync();

                // userId và product là các features là các data dựa vào để dự đoán, label là cột trả về kết quả dự đoán
                var getFeatureOrder = cthoadons.Select(p => new ProductRating
                {
                    userId = p.MaHdNavigation.MaKh.Value,
                    productId = p.MaCtsp.Value,
                    Label = p.SoLuong,
                });

                // Nạp dữ liệu từ một danh sách hoặc mảng vào một IDataView - định dạng dữ liệu mà ML.NET sử dụng để xử lý
                var data = mlContext.Data.LoadFromEnumerable(getFeatureOrder);

                /*  Sử dụng MapValueKey để chuyển đổi giá trị trong 2 cột userId và productId thành key (numeric key type) - tức là các giá trị số nguyên không âm liên tiếp
                là định dạng sử dụng trong thuật toán Matrix Factorization*/
                IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userId", inputColumnName: "userId")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "productId", inputColumnName: "productId"));

                var options = new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = "userId", // Cột là User
                    MatrixRowIndexColumnName = "productId", // Hàng là Product
                    LabelColumnName = "Label", // giá trị tương tác
                    NumberOfIterations = 20, // Số lần lặp (số lần Matrix Factorization chạy qua data) để huấn luyện mô hình
                    ApproximationRank = 5 // Số yếu tố tiềm ẩn, giá trị này càng lớn mô hình học càng nhiều đặc trưng hơn nhưng tốn time huấn luyện hơn
                };
                var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
                Console.WriteLine("=============== Training the model ===============");
                model = trainerEstimator.Fit(data);

                var trainTestSplit = mlContext.Data.TrainTestSplit(data, testFraction: 0.2); // 20% test, 80% train
                //Dữ liệu train
                var trainData = trainTestSplit.TrainSet;
                //Dữ liệu test
                var testData = trainTestSplit.TestSet;

                // Dự đoán trên tệp kiểm tra
                var predictions = model.Transform(testData);

                // So sánh Score (dự đoán) và label (thực tế)
                var metrics = mlContext.Recommendation().Evaluate(predictions, labelColumnName: "Label");
                Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");
                Console.WriteLine($"MAE: {metrics.MeanAbsoluteError}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi huấn luyện mô hình: {ex.Message}");
                throw;
            }

        }
        public async Task<List<ProductResponseDTO>> Recommend(int? userId, int? maSp = null, int numberOfRecommendations = 8)
        {
            await TrainModel();
            var userPurchases = await db.Cthoadons
                .AsNoTracking()
                .Include(p => p.MaHdNavigation)
                .Where(p => p.MaHdNavigation.MaKh == userId && p.MaCtsp.HasValue)
                .Select(p => p.MaCtsp.Value)
                .ToListAsync();

            var productToExclude = new List<int>();

            if (maSp.HasValue)
            {
                productToExclude = await db.Chitietsanphams
                    .AsNoTracking()
                    .Where(p => p.MaSp == maSp.Value)
                    .Select(p => p.MaCtsp)
                    .ToListAsync();
            }
            var excludedProductIds = userPurchases.Concat(productToExclude).Distinct().ToHashSet();

            var allProducts = await db.Cthoadons
                .AsNoTracking()
                .Where(p => p.MaCtsp.HasValue)
                .Select(p => p.MaCtsp.Value)
                .Distinct()
                .ToListAsync();

            var productsToRecommend = allProducts.Except(excludedProductIds).ToList();


            var predictions = new List<(int ProductId, float Score)>();
            // Tạo một prediction engine để dự đoán giá trị cho các cặp userid-productid
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ProductRating, ProductRatingPrediction>(model);

            foreach (var productId in productsToRecommend)
            {
                // Sử dụng PredictionEngine
                var prediction = predictionEngine.Predict(new ProductRating
                {
                    userId = userId.Value,
                    productId = productId
                });
                Console.WriteLine($"UserId: {userId.Value}, ProductId: {productId}, Score: {prediction.Score}");
                predictions.Add((productId, prediction.Score));
            }
            // Bước 3: Lấy danh sách sản phẩm mà các khách hàng liên quan đã mua
            var idProductRecommend = predictions
                .OrderByDescending(p => p.Score)
                .Take(numberOfRecommendations)
                .ToList();

            var ListProductRecommend = new List<ProductResponseDTO>();
           if (idProductRecommend.Count > 0)
           {
                foreach (var product in idProductRecommend)
                {
                    var detailproducts = await db.Chitietsanphams.AsNoTracking().FirstOrDefaultAsync(p => p.MaCtsp == product.ProductId);
                    var findproduct = await db.Sanphams.AsNoTracking()
                        .Where(p => p.IsActive == true)
                        .Include(p => p.Chitietsanphams)
                        .ThenInclude(p => p.Hinhanhs)
                        .FirstOrDefaultAsync(p => p.MaSp == detailproducts.MaSp);
                    var checkListProductRecommend = ListProductRecommend.FirstOrDefault(p => p.MaSp == findproduct.MaSp);
                    if (checkListProductRecommend != null)
                    {
                        continue;
                    }
                    ListProductRecommend.Add(new ProductResponseDTO
                    {
                        MaSp = findproduct.MaSp,
                        TenSanPham = findproduct.TenSanPham,
                        SoLuong = (int)findproduct.Chitietsanphams.Where(p => p.IsActive == true).Sum(p => p.SoLuongTon),
                        KhoangGia = findproduct.Chitietsanphams.Where(p => p.IsActive == true).Any()
                    ? (findproduct.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia) == findproduct.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)
                        ? $"{findproduct.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ"
                        : $"{findproduct.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ - {findproduct.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)} VNĐ")
                    : "Chưa có giá",
                        MoTa = findproduct.MoTa,
                        ProductDetails = findproduct.Chitietsanphams.Where(p => p.IsActive == true).Select(details => new ProductDetailResponseDTO
                        {
                            MaCtsp = details.MaCtsp,
                            KichThuoc = string.IsNullOrEmpty(details.KichThuoc) == true ? "" : details.KichThuoc,
                            MauSac = string.IsNullOrEmpty(details.MauSac) == true ? "" : details.MauSac,
                            SoLuongTon = details.SoLuongTon,
                            DonGia = details.DonGia,
                            Images = details.Hinhanhs
                            .Select(image => new ImageProductResponseDTO
                            {
                                TenHinhAnh = image.TenHinhAnh,
                                MaCtsp = image.MaCtsp,
                            })
                            .ToList(),
                        }).ToList(),
                    });
                }
            }
            return ListProductRecommend;
        }
    }
}
