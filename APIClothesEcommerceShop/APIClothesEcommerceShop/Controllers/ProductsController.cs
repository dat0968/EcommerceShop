using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Nhân viên")]
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
        public async Task<IActionResult> Index(string? search, string? selectedBigCategory, string? selectedSmallCategory, string? sortByPrice, string? filterPrice, int page = 1)
        {
            try
            {
                page = page < 1 ? 1 : page;
                int pagesize = 10;
                var ListProduct = await ProductRepository.GetAll(search, selectedBigCategory, selectedSmallCategory, sortByPrice, filterPrice);
                var ListProductByPage = ListProduct.Skip((page - 1) * pagesize).Take(pagesize);
                return Ok(new
                {
                    Success = true,
                    Data = ListProductByPage,
                    ToTalPages = (int)Math.Ceiling((double)ListProduct.Count() / pagesize),
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Details([FromRoute] int id)
        {
            try
            {
                var Product = await ProductRepository.GetById(id);
                if (Product == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }
                return Ok(new
                {
                    Success = true,
                    Data = Product
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductResquestDTO model)
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] ProductResquestDTO model)
        {
            try
            {
                var Product = await productService.UpdateProduct(id, model);
                return Ok(new
                {
                    Success = true,
                    Data = Product
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpPut("{id}/Cancel")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await ProductRepository.Cancel(id);
                return Ok(new
                {
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("xuat-excel")]
        public async Task<IActionResult> XuatExcel()
        {
            var products = await ProductRepository.GetAll(null, null, null, null, null);
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachSanPham");

            // Header
            var row = 1;
            worksheet.Cell(row, 1).Value = "Mã SP";
            worksheet.Cell(row, 2).Value = "Tên Sản phẩm";
            worksheet.Cell(row, 3).Value = "Kích thước";
            worksheet.Cell(row, 4).Value = "Màu sắc";
            worksheet.Cell(row, 5).Value = "Giá bán";
            worksheet.Cell(row, 6).Value = "Số lượng";
            worksheet.Cell(row, 7).Value = "Mô tả";
            worksheet.Cell(row, 8).Value = "Ngày tạo";

            foreach (var sp in products)
            {
                foreach (var ct in sp.ProductDetails)
                {
                    row++;
                    worksheet.Cell(row, 1).Value = sp.MaSp;
                    worksheet.Cell(row, 2).Value = sp.TenSanPham;
                    worksheet.Cell(row, 3).Value = ct.KichThuoc;
                    worksheet.Cell(row, 4).Value = ct.MauSac;
                    worksheet.Cell(row, 5).Value = ct.DonGia;
                    worksheet.Cell(row, 6).Value = ct.SoLuongTon;
                    worksheet.Cell(row, 7).Value = sp.MoTa;
                    worksheet.Cell(row, 8).Value = sp.NgayTao.ToString("dd/MM/yyyy");
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachSanPham.xlsx");

        }
    }
}
