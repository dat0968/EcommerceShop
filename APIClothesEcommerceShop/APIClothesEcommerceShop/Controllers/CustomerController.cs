using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Customer;
using APIClothesEcommerceShop.Repositories.Customer;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Geom;
using ClosedXML.Excel;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CustomerController(ICustomerRepository customerRepository, IWebHostEnvironment webHostEnvironment)
    {
        _customerRepository = customerRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string hoTen = null,
        [FromQuery] string gioiTinh = null,
        [FromQuery] string tinhTrang = null)
    {
        try
        {
            var customers = await _customerRepository.GetAllCustomersAsync(pageSize, pageNumber, hoTen, gioiTinh, tinhTrang);
            var totalCount = await _customerRepository.GetCustomersCountAsync(hoTen, gioiTinh, tinhTrang);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");

            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCustomersCount(
        [FromQuery] string hoTen = null,
        [FromQuery] string gioiTinh = null,
        [FromQuery] string tinhTrang = null)
    {
        try
        {
            var count = await _customerRepository.GetCustomersCountAsync(hoTen, gioiTinh, tinhTrang);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("{maKH}")]
    public async Task<IActionResult> GetCustomerById(int maKH)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(maKH);
            if (customer == null) return NotFound("Khách hàng không tồn tại.");
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCustomer([FromForm] CustomerDto customerDto)
    {
        try
        {
            var result = await _customerRepository.AddCustomerAsync(customerDto);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage); // Sửa Message thành ErrorMessage
            return Ok(result.ErrorMessage); // Sửa Message thành ErrorMessage
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string hoTen = null,
        [FromQuery] string sdt = null,
        [FromQuery] string email = null,
        [FromQuery] string cccd = null,
        [FromQuery] string diaChi = null)
    {
        try
        {
            var customers = await _customerRepository.SearchCustomersAsync(pageSize, pageNumber, hoTen, sdt, email, cccd, diaChi);
            var totalCount = await _customerRepository.GetSearchCountAsync(hoTen, sdt, email, cccd, diaChi);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");

            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpPut("{maKH}")]
    public async Task<IActionResult> UpdateCustomer(int maKH, [FromForm] CustomerDto customerDto)
    {
        try
        {
            var result = await _customerRepository.UpdateCustomerAsync(maKH, customerDto);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage); // Sửa Message thành ErrorMessage
            return Ok(result.ErrorMessage); // Sửa Message thành ErrorMessage
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpDelete("{maKH}")]
    public async Task<IActionResult> DeleteCustomer(int maKH)
    {
        try
        {
            var result = await _customerRepository.DeleteCustomerAsync(maKH);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage); // Sửa Message thành ErrorMessage
            return Ok(result.ErrorMessage); // Sửa Message thành ErrorMessage
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportCustomersToPdf()
    {
        try
        {
            var customers = await _customerRepository.GetCustomersForExportAsync();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A3);

                // Kiểm tra sự tồn tại của file font
                string fontPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "fonts", "arial.ttf");
                if (!System.IO.File.Exists(fontPath))
                {
                    return BadRequest("Không tìm thấy file font tại đường dẫn: " + fontPath);
                }

                var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, pdf);
                document.SetFont(font);

                document.Add(new Paragraph("Danh Sách Khách Hàng")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20f));

                var table = new Table(UnitValue.CreatePercentArray(new float[] { 25f, 15f, 15f, 20f, 15f, 20f, 15f, 15f }))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                table.AddHeaderCell(new Cell().Add(new Paragraph("Họ Tên").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Giới Tính").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày Sinh").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Địa Chỉ").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("SĐT").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày Tạo").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tình Trạng").SetFont(font)));

                foreach (var customer in customers)
                {
                    table.AddCell(new Cell().Add(new Paragraph(customer.HoTen ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.GioiTinh ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.NgaySinh == DateTime.MinValue ? "" : customer.NgaySinh.ToString("dd/MM/yyyy")).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.DiaChi ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.SDT ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.Email ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.NgayTao.HasValue ? customer.NgayTao.Value.ToString("dd/MM/yyyy") : "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(customer.TinhTrang ?? "").SetFont(font)));
                }

                document.Add(table);
                document.Close();

                var pdfBytes = memoryStream.ToArray();
                return File(pdfBytes, "application/pdf", "DanhSachKhachHang.pdf");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server khi xuất PDF: {ex.Message}");
        }
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportCustomersToExcel()
    {
        try
        {
            var customers = await _customerRepository.GetCustomersForExportAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Khách Hàng");

                worksheet.Cell(1, 1).Value = "Họ Tên";
                worksheet.Cell(1, 2).Value = "Giới Tính";
                worksheet.Cell(1, 3).Value = "Ngày Sinh";
                worksheet.Cell(1, 4).Value = "Địa Chỉ";
                worksheet.Cell(1, 5).Value = "SĐT";
                worksheet.Cell(1, 6).Value = "Email";
                worksheet.Cell(1, 7).Value = "Ngày Tạo";
                worksheet.Cell(1, 8).Value = "Tình Trạng";

                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;
                foreach (var customer in customers)
                {
                    worksheet.Cell(row, 1).Value = customer.HoTen ?? "";
                    worksheet.Cell(row, 2).Value = customer.GioiTinh ?? "";
                    worksheet.Cell(row, 3).Value = customer.NgaySinh == DateTime.MinValue ? "" : customer.NgaySinh.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 4).Value = customer.DiaChi ?? "";
                    worksheet.Cell(row, 5).Value = customer.SDT ?? "";
                    worksheet.Cell(row, 6).Value = customer.Email ?? "";
                    worksheet.Cell(row, 7).Value = customer.NgayTao.HasValue ? customer.NgayTao.Value.ToString("dd/MM/yyyy") : "";
                    worksheet.Cell(row, 8).Value = customer.TinhTrang ?? "";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var tableRange = worksheet.Range(1, 1, row - 1, 8);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Position = 0;

                    return File(
                        memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "DanhSachKhachHang.xlsx");
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server khi xuất Excel: {ex.Message}");
        }
    }
    [HttpGet("image/{fileName}")]
    public IActionResult GetCustomerImage(string fileName)
    {
        try
        {
            // Xây dựng đường dẫn đến hình ảnh
            var imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "AnhKhachHang", fileName);

            // Kiểm tra xem file có tồn tại không
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Không tìm thấy hình ảnh");
            }

            // Xác định loại MIME dựa trên phần mở rộng
            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            string contentType = "application/octet-stream"; // Giá trị mặc định

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
            }

            // Đọc và trả về file
            var fileBytes = System.IO.File.ReadAllBytes(imagePath);
            return File(fileBytes, contentType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi tải hình ảnh: {ex.Message}");
        }
    }
}