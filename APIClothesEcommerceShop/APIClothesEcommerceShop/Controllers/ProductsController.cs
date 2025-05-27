using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService productService;
        private readonly IProductRepository ProductRepository;
        public ProductsController(ProductService productService, IProductRepository ProductRepository)
        {
            this.productService = productService;
            this.ProductRepository = ProductRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                page = page < 1 ? 1 : page;
                int pagesize = 10;
                var ListProduct = await ProductRepository.GetAll();
                var ListProductByPage = ListProduct.Skip((page - 1) * pagesize).Take(pagesize);
                return Ok(new
                {
                    Success = true,
                    Data = ListProductByPage,
                    ToTalPages = (int)Math.Ceiling((double)ListProduct.Count() / pagesize),
                });
            }catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]ProductResquestDTO model)
        {
            try
            {
                var ListProduct = await productService.AddProduct(model);
                return Ok(new
                {
                    Success = true,
                    Data = ListProduct
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
    }
}
