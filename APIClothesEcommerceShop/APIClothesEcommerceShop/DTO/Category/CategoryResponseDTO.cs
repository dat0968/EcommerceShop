using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Category
{
    public class CategoryResponseDTO
    {
        public int MaDanhMucCha { get; set; }

        public string TenDanhMucCha { get; set; } = null!;

        public bool? IsActive { get; set; }

        public virtual ICollection<CategoryDetailsResponseDTO> Chitietdanhmucs { get; set; } = new List<CategoryDetailsResponseDTO>();
    }
}
