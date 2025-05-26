using System;
using System.Collections.Generic;
using APIClothesEcommerceShop.DTO.Categories;

namespace APIClothesEcommerceShop.Models;

public partial class Danhmuccha
{
    public int MaDanhMucCha { get; set; }

    public string TenDanhMucCha { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietdanhmuc> Chitietdanhmucs { get; set; } = new List<Chitietdanhmuc>();
}
public static class DanhmucchaExtensions
{
    public static CategoryParentResponseDTO ToCategoryParentResponseDTO(this Danhmuccha entity)
    {
        return new CategoryParentResponseDTO
        {
            MaDanhMucCha = entity.MaDanhMucCha,
            TenDanhMucCha = entity.TenDanhMucCha,
            IsActive = entity.IsActive
        };
    }
}