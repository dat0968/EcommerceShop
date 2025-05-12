using APIClothesEcommerceShop.DTO.ImageProduct;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.ProductDetails
{
    public class AddProductDetailResponseDTO
    {
        public string? KichThuoc { get; set; }

        public string? MauSac { get; set; }
      
        public int SoLuongTon { get; set; }

        public int DonGia { get; set; }

        public bool? IsActive { get; set; }

        public virtual ICollection<ImageProductResponseDTO> Images { get; set; } = new List<ImageProductResponseDTO>();
    }
}
