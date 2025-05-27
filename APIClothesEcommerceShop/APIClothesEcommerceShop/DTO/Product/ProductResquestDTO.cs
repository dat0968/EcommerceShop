using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace APIClothesEcommerceShop.DTO.Product
{
    public class ProductResquestDTO
    {
        public string TenSanPham { get; set; } = null!;
        public string? MoTa { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<CategoryDetailsResponseDTO> CategoryDetails { get; set; } = new List<CategoryDetailsResponseDTO>();

        public virtual ICollection<ProductDetailRequestDTO> ProductDetails { get; set; } = new List<ProductDetailRequestDTO>();

    }
}
