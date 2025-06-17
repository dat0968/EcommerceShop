using APIClothesEcommerceShop.DTO.Shop;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories.Combos;
using APIClothesEcommerceShop.Repositories.Home;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Azure.Core.HttpHeader;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IProductRepository ProductRepository;
        private readonly IComboRepository comboRepository;
        public ShopController(IProductRepository ProductRepository, IComboRepository comboRepository) {
            this.ProductRepository = ProductRepository;
            this.comboRepository = comboRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? selectedBigCategory, string? selectedSmallCategory, string? sortByPrice, string? filterPrice, int page = 1)
        {
            try
            {
                page = page < 1 ? 1 : page;
                int pagesize = 12;
                var ListProduct = await ProductRepository.GetAll(search, selectedBigCategory, selectedSmallCategory, sortByPrice, filterPrice);
                var ListCombo = await comboRepository.GetAll(search);

                var productQuery = ListProduct.Select(p => new ShopItemDTO
                {
                    Id = p.MaSp,
                    Name = p.TenSanPham,
                    Type = "Product",
                    Image = p.ProductDetails.FirstOrDefault()?.Images.FirstOrDefault()?.TenHinhAnh ?? "", 
                    PriceRange = p.KhoangGia,
                    DiscountPercentage = null,
                    DiscountAmount = null,
                });
                var comboQuery = ListCombo
                     .Where(c => c.IsActive == true && c.NgayBatDau <= DateTime.Now && c.NgayKetThuc >= DateTime.Now)
                     .Select(c => new ShopItemDTO
                     {
                         Id = c.MaCombo,
                         Name = c.TenCombo,
                         Type = "Combo",
                         Image = c.Hinh ?? "",
                         PriceRange = null,
                         DiscountPercentage = c.PhanTramGiam,
                         DiscountAmount = c.SoTienGiam,
                     });
                var combinedItems = productQuery.Concat(comboQuery).ToList();
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
        public async Task<IActionResult> DetailsProduct(int id)
        {
            try
            {
                var details = await ProductRepository.GetById(id);
                if (details == null)
                {
                    return NotFound(new { message = "Sản phẩm không tồn tại" });
                }

                var productToUpdate = new Sanpham
                {
                    MaSp = details.MaSp,
                    TenSanPham = details.TenSanPham,
                    NgayTao = details.NgayTao,
                    LuotXem = details.LuotXem + 1, 
                    MoTa = details.MoTa,
                    IsActive = true,
                };

                await ProductRepository.Update(productToUpdate);
                var updatedDetails = await ProductRepository.GetById(id);
                return Ok(updatedDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("Combo/{id}")]
        public async Task<IActionResult> DetailsCombo(int id)
        {
            try
            {
                var details = await comboRepository.GetById(id);
                if (details == null)
                {
                    return NotFound(new { message = "Combo tồn tại" });
                }
                return Ok(details);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
    }
}
