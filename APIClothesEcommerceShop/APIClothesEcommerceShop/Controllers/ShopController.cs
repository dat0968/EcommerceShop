using APIClothesEcommerceShop.DTO.Shop;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories.Home;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using APIClothesEcommerceShop.Data;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Drawing;
using APIClothesEcommerceShop.Repositories.Combos;
using APIClothesEcommerceShop.Repositories.ViewHistory;
using System.Runtime.CompilerServices;
using System;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IComboRepository _comboRepository;
        private readonly EcommerceShopContext _db;
        private readonly IViewHistoryRepository viewHistoryRepository;
        public ShopController(
            IProductRepository productRepository,
            IComboRepository comboRepository,
            IViewHistoryRepository viewHistoryRepository,
            EcommerceShopContext db)
        {
             this.viewHistoryRepository = viewHistoryRepository;
            _productRepository = productRepository;
            _comboRepository = comboRepository;
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? selectedBigCategory,
            string? selectedSmallCategory,
            string? sortByPrice,
            string? filterPrice,
            bool isCombo = false,
            int page = 1)
        {
            try
            {
                page = page < 1 ? 1 : page;
                int pagesize = 12;
                var ListProduct = await _productRepository.GetAll(search, selectedBigCategory, selectedSmallCategory, sortByPrice, filterPrice);
                var ListCombo = await _comboRepository.GetAll(search);
                var combinedItems = new List<ShopItemDTO>();
                var productQuery = ListProduct.Select(p => new ShopItemDTO
                {
                    Id = p.MaSp,
                    Name = p.TenSanPham,
                    Type = "Product",
                    Image = p.AnhDaiDien,
                    PriceRange = p.KhoangGia,
                    DiscountPercentage = null,
                    DiscountAmount = null,
                    AverageRating = p.AverageRating,
                    ReviewCount = p.ReviewCount
                });
                var comboQuery = ListCombo
                     .Where(c => c.NgayBatDau <= DateTime.Now && c.NgayKetThuc >= DateTime.Now)
                     .Select(c => new ShopItemDTO
                     {
                         Id = c.MaCombo,
                         Name = c.TenCombo,
                         Type = "Combo",
                         Image = c.Hinh ?? "",
                         PriceRange = null,
                         DiscountPercentage = c.PhanTramGiam,
                         DiscountAmount = c.SoTienGiam,
                         AverageRating = c.AverageRating,
                         ReviewCount = c.ReviewCount
                     });
                if (!isCombo)
                {
                    combinedItems = productQuery.Concat(comboQuery).ToList();
                }
                else
                {
                    combinedItems = comboQuery.ToList();
                }
                var ListProductByPage = combinedItems.Skip((page - 1) * pagesize).Take(pagesize);
                return Ok(new
                {
                    Success = true,
                    Data = ListProductByPage,
                    ToTalPages = (int)Math.Ceiling((double)combinedItems.Count() / pagesize),
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("Product/{id}")]
        public async Task<IActionResult> DetailsProduct([FromQuery] int? maKh, int id)
        {
            try
            {
                var details = await _productRepository.GetById(id);
                if (details == null)
                {
                    return NotFound(new { message = "Sản phẩm không tồn tại" });
                }
                if (maKh.HasValue)
                {
                    await viewHistoryRepository.AddOrUpdateAsync(maKh.Value, id, null);
                }
                return Ok(details);
            }
            catch (Exception)
            {
                return BadRequest(new { Success = false, Message = "Lỗi tải chi tiết sản phẩm" });
            }
        }
        [HttpGet("Combo/{id}")]
        public async Task<IActionResult> DetailsCombo(int? maKh, int id)
        {
            try
            {
                var details = await _comboRepository.GetById(id);
                if (details == null || (details.NgayBatDau > DateTime.Now || details.NgayKetThuc < DateTime.Now ) )
                {
                    return NotFound(new { message = "Combo tồn tại" });
                }
                if (maKh.HasValue)
                {
                    await viewHistoryRepository.AddOrUpdateAsync(maKh.Value, null, id);
                }
                return Ok(details);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}