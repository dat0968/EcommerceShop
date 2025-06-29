using APIClothesEcommerceShop.DTO.Shop;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using APIClothesEcommerceShop.Data;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Controllers
{
    // Helper DTO class để tránh anonymous type
    public class ProductMinimalDto
    {
        public int MaSp { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool HasPrice { get; set; }
        public string? FirstImage { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IComboRepository _comboRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ShopController> _logger;
        private readonly EcommerceShopContext _db;

        public ShopController(
            IProductRepository productRepository,
            IComboRepository comboRepository,
            IMemoryCache cache,
            ILogger<ShopController> logger,
            EcommerceShopContext db)
        {
            _productRepository = productRepository;
            _comboRepository = comboRepository;
            _cache = cache;
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? selectedBigCategory,
            string? selectedSmallCategory,
            string? sortByPrice,
            string? filterPrice,
            int page = 1)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                page = page < 1 ? 1 : page;
                const int pagesize = 12;

                var cacheKey = $"shop_fast_{search ?? "null"}_{selectedBigCategory ?? "null"}_{selectedSmallCategory ?? "null"}_{sortByPrice ?? "null"}_{filterPrice ?? "null"}_{page}";

                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    _logger.LogInformation("Shop loaded from cache in {LoadTime}ms", (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return Ok(cachedResult);
                }

                // FAST LOADING: Chỉ lấy data tối thiểu cần thiết cho shop list
                var productTask = GetFastProducts(search, selectedBigCategory, selectedSmallCategory, sortByPrice, filterPrice);
                var comboTask = GetFastCombos(search);

                await Task.WhenAll(productTask, comboTask);

                var products = await productTask;
                var combos = await comboTask;

                var totalItems = products.Count + combos.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pagesize);

                // Combine và paginate
                var allItems = new List<ShopItemDTO>(totalItems);
                allItems.AddRange(products);
                allItems.AddRange(combos);

                var pagedItems = allItems
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
                    .ToList();

                var result = new
                {
                    Success = true,
                    Data = pagedItems,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    PageNumber = page,
                    PageSize = pagesize
                };

                // Cache 5 phút
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

                var loadTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("Shop loaded in {LoadTime}ms - Products: {ProductCount}, Combos: {ComboCount}",
                    loadTime, products.Count, combos.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Shop error after {LoadTime}ms", errorTime);
                return BadRequest(new { Success = false, Message = "Lỗi tải dữ liệu shop" });
            }
        }

        // FAST PRODUCTS: Chỉ lấy data cần thiết, không load tất cả relationships
        private async Task<List<ShopItemDTO>> GetFastProducts(
            string? search,
            string? selectedBigCategory,
            string? selectedSmallCategory,
            string? sortByPrice,
            string? filterPrice)
        {
            try
            {
                var query = _db.Sanphams.AsNoTracking()
                    .Where(p => p.IsActive == true);

                // Apply search
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.TenSanPham.Contains(search) || p.MaSp.ToString().Contains(search));
                }

                // Apply category filters
                if (!string.IsNullOrEmpty(selectedBigCategory) && int.TryParse(selectedBigCategory, out int bigCatId))
                {
                    query = query.Where(p => p.Chitietdanhmucs.Any(cd => cd.MaDanhMucCha == bigCatId));
                }

                if (!string.IsNullOrEmpty(selectedSmallCategory) && int.TryParse(selectedSmallCategory, out int smallCatId))
                {
                    query = query.Where(p => p.Chitietdanhmucs.Any(cd => cd.MaDanhMucCon == smallCatId));
                }

                // Lấy data với minimal includes
                var products = await query
                    .Select(p => new ProductMinimalDto
                    {
                        MaSp = p.MaSp,
                        TenSanPham = p.TenSanPham,
                        // Lấy giá min/max trực tiếp từ database
                        MinPrice = p.Chitietsanphams.Where(ct => ct.IsActive == true).Any()
                            ? p.Chitietsanphams.Where(ct => ct.IsActive == true).Min(ct => ct.DonGia)
                            : 0,
                        MaxPrice = p.Chitietsanphams.Where(ct => ct.IsActive == true).Any()
                            ? p.Chitietsanphams.Where(ct => ct.IsActive == true).Max(ct => ct.DonGia)
                            : 0,
                        HasPrice = p.Chitietsanphams.Any(ct => ct.IsActive == true),
                        // Lấy hình đầu tiên
                        FirstImage = p.Chitietsanphams
                            .Where(ct => ct.IsActive == true)
                            .SelectMany(ct => ct.Hinhanhs)
                            .Select(h => h.TenHinhAnh)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                var shopItems = products.Select(p => new ShopItemDTO
                {
                    Id = p.MaSp,
                    Name = p.TenSanPham,
                    Type = "Product",
                    Image = p.FirstImage ?? "",
                    PriceRange = p.HasPrice
                        ? (p.MinPrice == p.MaxPrice
                            ? $"{p.MinPrice:N0} VNĐ"
                            : $"{p.MinPrice:N0} VNĐ - {p.MaxPrice:N0} VNĐ")
                        : "Chưa có giá",
                    DiscountPercentage = null,
                    DiscountAmount = null
                }).ToList();

                // Apply price filter in memory (faster than complex DB query)
                if (!string.IsNullOrEmpty(filterPrice))
                {
                    shopItems = ApplyPriceFilter(shopItems, filterPrice, products);
                }

                // Apply sorting in memory
                if (!string.IsNullOrEmpty(sortByPrice))
                {
                    shopItems = ApplySorting(shopItems, sortByPrice, products);
                }

                return shopItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFastProducts");
                return new List<ShopItemDTO>();
            }
        }

        // FAST COMBOS: Tối ưu query combo
        private async Task<List<ShopItemDTO>> GetFastCombos(string? search)
        {
            try
            {
                var now = DateTime.Now;
                var query = _db.Combos.AsNoTracking()
                    .Where(c => c.IsActive == true && c.NgayBatDau <= now && c.NgayKetThuc >= now);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c => c.TenCombo.Contains(search));
                }

                var combos = await query
                    .Select(c => new ShopItemDTO
                    {
                        Id = c.MaCombo,
                        Name = c.TenCombo,
                        Type = "Combo",
                        Image = c.Hinh ?? "",
                        PriceRange = null,
                        DiscountPercentage = c.PhanTramGiam,
                        DiscountAmount = c.SoTienGiam
                    })
                    .ToListAsync();

                return combos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFastCombos");
                return new List<ShopItemDTO>();
            }
        }

        private static List<ShopItemDTO> ApplyPriceFilter(List<ShopItemDTO> items, string filterPrice, List<ProductMinimalDto> products)
        {
            var productDict = products.ToDictionary(p => p.MaSp, p => p.MaxPrice);

            return filterPrice.ToLower() switch
            {
                "dưới 300k" => items.Where(i => i.Type != "Product" || productDict.GetValueOrDefault(i.Id, 0) < 300000).ToList(),
                "300k - 1 triệu" => items.Where(i => i.Type != "Product" || (productDict.GetValueOrDefault(i.Id, 0) >= 300000 && productDict.GetValueOrDefault(i.Id, 0) <= 1000000)).ToList(),
                "1 triệu - 2 triệu" => items.Where(i => i.Type != "Product" || (productDict.GetValueOrDefault(i.Id, 0) >= 1000000 && productDict.GetValueOrDefault(i.Id, 0) <= 2000000)).ToList(),
                "trên 2 triệu" => items.Where(i => i.Type != "Product" || productDict.GetValueOrDefault(i.Id, 0) >= 2000000).ToList(),
                _ => items
            };
        }

        private static List<ShopItemDTO> ApplySorting(List<ShopItemDTO> items, string sortByPrice, List<ProductMinimalDto> products)
        {
            var productDict = products.ToDictionary(p => p.MaSp, p => p.MinPrice);

            return sortByPrice.ToLower() switch
            {
                "asc" => items.OrderBy(i => i.Type == "Product" ? productDict.GetValueOrDefault(i.Id, 0) : 0).ToList(),
                "desc" => items.OrderByDescending(i => i.Type == "Product" ? productDict.GetValueOrDefault(i.Id, 0) : 0).ToList(),
                _ => items
            };
        }

        [HttpGet("Product/{id}")]
        public async Task<IActionResult> DetailsProduct(int id)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                var cacheKey = $"product_detail_{id}";

                if (_cache.TryGetValue(cacheKey, out var cachedProduct))
                {
                    // Async view count update
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _db.Database.ExecuteSqlRawAsync(
                                "UPDATE Sanphams SET LuotXem = LuotXem + 1 WHERE MaSp = {0}", id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to update view count for product {ProductId}", id);
                        }
                    });

                    _logger.LogInformation("Product {ProductId} from cache in {LoadTime}ms", id, (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return Ok(cachedProduct);
                }

                var details = await _productRepository.GetById(id);
                if (details == null)
                {
                    return NotFound(new { message = "Sản phẩm không tồn tại" });
                }

                _cache.Set(cacheKey, details, TimeSpan.FromMinutes(3));

                // Async view count update
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _db.Database.ExecuteSqlRawAsync(
                            "UPDATE Sanphams SET LuotXem = LuotXem + 1 WHERE MaSp = {0}", id);
                        _cache.Remove(cacheKey); // Remove cache after update
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update view count for product {ProductId}", id);
                    }
                });

                _logger.LogInformation("Product {ProductId} from DB in {LoadTime}ms", id, (DateTime.UtcNow - startTime).TotalMilliseconds);
                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product {ProductId}", id);
                return BadRequest(new { Success = false, Message = "Lỗi tải chi tiết sản phẩm" });
            }
        }

        [HttpGet("Combo/{id}")]
        public async Task<IActionResult> DetailsCombo(int id)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                var cacheKey = $"combo_detail_{id}";

                if (_cache.TryGetValue(cacheKey, out var cachedCombo))
                {
                    _logger.LogInformation("Combo {ComboId} from cache in {LoadTime}ms", id, (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return Ok(cachedCombo);
                }

                var details = await _comboRepository.GetById(id);
                if (details == null)
                {
                    return NotFound(new { message = "Combo không tồn tại" });
                }

                _cache.Set(cacheKey, details, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Combo {ComboId} from DB in {LoadTime}ms", id, (DateTime.UtcNow - startTime).TotalMilliseconds);

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading combo {ComboId}", id);
                return BadRequest(new { Success = false, Message = "Lỗi tải chi tiết combo" });
            }
        }
    }
}