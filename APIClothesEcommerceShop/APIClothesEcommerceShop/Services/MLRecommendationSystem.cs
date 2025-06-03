using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.RecommendProduct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace APIClothesEcommerceShop.Services
{
    public class MLRecommendationSystem
    {
        private readonly MLContext mlContext;
        private readonly EcommerceShopContext db;
        public MLRecommendationSystem(EcommerceShopContext db)
        {
            mlContext = new MLContext();
            this.db = db;
        }
        private async Task TrainModel()
        {
            // Truy suất dữ liệu hóa đơn
            var cthoadons = await db.Cthoadons.AsNoTracking()
                            .Where(p => p.MaCtsp.HasValue && p.MaHdNavigation != null && p.MaHdNavigation.MaKh.HasValue).ToListAsync();

            // userId và product là các features là các data dựa vào để dự đoán, label là cột trả về kết quả dự đoán
            var getFeatureOrder = cthoadons.Select(p => new ProductRating
            {
                userId = p.MaHdNavigation.MaKh.Value,
                productId = p.MaCtsp.Value,
                Label = p.MaHdNavigation.TinhTrang.ToLower() == "đã thanh toán" ? 1.0f : 0.0f  
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
                NumberOfIterations = 20,
                ApproximationRank = 100
            };


        }
    }
}
