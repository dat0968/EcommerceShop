
namespace APIClothesEcommerceShop.DTO.TryOn
{
    /// <summary>
    /// Đại diện cho yêu cầu xử lý và tải lên hình ảnh, bao gồm ảnh người mẫu và ảnh sản phẩm.
    /// </summary>
    public class ProcessImageRequest
    {
        /// <summary>
        /// Chuỗi Base64 của hình ảnh người mẫu (model image). Định dạng: data:image/jpeg;base64,...
        /// </summary>
        public string ModelImageBase64 { get; set; }

        /// <summary>
        /// Danh sách các chuỗi Base64 của hình ảnh sản phẩm. Định dạng: data:image/jpeg;base64,...
        /// Hiện tại, chỉ ảnh sản phẩm đầu tiên trong danh sách được sử dụng bởi LightX.
        /// </summary>
        public List<string> ProductImagesBase64 { get; set; }
    }
}
