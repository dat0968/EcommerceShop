using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Staff;
using APIClothesEcommerceShop.Repositories.Staff;
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
public class StaffController : ControllerBase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public StaffController(IStaffRepository staffRepository, IWebHostEnvironment webHostEnvironment)
    {
        _staffRepository = staffRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStaff(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string hoTen = null,
        [FromQuery] string gioiTinh = null,
        [FromQuery] string tinhTrang = null)
    {
        try
        {
            var staff = await _staffRepository.GetAllStaffAsync(pageSize, pageNumber, hoTen, gioiTinh, tinhTrang);
            var totalCount = await _staffRepository.GetStaffCountAsync(hoTen, gioiTinh, tinhTrang);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");

            return Ok(staff);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetStaffCount(
        [FromQuery] string hoTen = null,
        [FromQuery] string gioiTinh = null,
        [FromQuery] string tinhTrang = null)
    {
        try
        {
            var count = await _staffRepository.GetStaffCountAsync(hoTen, gioiTinh, tinhTrang);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("{maNV}")]
    public async Task<IActionResult> GetStaffById(int maNV)
    {
        try
        {
            var staff = await _staffRepository.GetStaffByIdAsync(maNV);
            if (staff == null) return NotFound("Nhân viên không tồn tại.");
            return Ok(staff);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddStaff([FromForm] StaffDto staffDto)
    {
        try
        {
            var result = await _staffRepository.AddStaffAsync(staffDto);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);
            return Ok(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchStaff(
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
            var staff = await _staffRepository.SearchStaffAsync(pageSize, pageNumber, hoTen, sdt, email, cccd, diaChi);
            var totalCount = await _staffRepository.GetSearchCountAsync(hoTen, sdt, email, cccd, diaChi);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");

            return Ok(staff);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpPut("{maNV}")]
    public async Task<IActionResult> UpdateStaff(int maNV, [FromForm] StaffDto staffDto)
    {
        try
        {
            var result = await _staffRepository.UpdateStaffAsync(maNV, staffDto);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);
            return Ok(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpDelete("{maNV}")]
    public async Task<IActionResult> DeleteStaff(int maNV)
    {
        try
        {
            var result = await _staffRepository.DeleteStaffAsync(maNV);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);
            return Ok(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }
    [HttpGet("chucvus")]
    public async Task<IActionResult> GetAllChucvus()
    {
        try
        {
            var chucvus = await _staffRepository.GetAllChucvusAsync();
            return Ok(chucvus);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportStaffToPdf()
    {
        try
        {
            var staffList = await _staffRepository.GetStaffForExportAsync();

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

                document.Add(new Paragraph("Danh Sách Nhân Viên")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20f));

                var table = new Table(UnitValue.CreatePercentArray(new float[] { 25f, 15f, 15f, 20f, 15f, 20f, 15f, 15f, 15f }))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                table.AddHeaderCell(new Cell().Add(new Paragraph("Họ Tên").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Giới Tính").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày Sinh").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Địa Chỉ").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("SĐT").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày Vào Làm").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tình Trạng").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Chức Vụ").SetFont(font)));

                foreach (var staff in staffList)
                {
                    table.AddCell(new Cell().Add(new Paragraph(staff.HoTen ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.GioiTinh ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.NgaySinh == DateTime.MinValue ? "" : staff.NgaySinh.ToString("dd/MM/yyyy")).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.DiaChi ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.SDT ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.Email ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.NgayVaoLam.HasValue ? staff.NgayVaoLam.Value.ToString("dd/MM/yyyy") : "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.TinhTrang ?? "").SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(staff.TenChucVu ?? "").SetFont(font)));
                }

                document.Add(table);
                document.Close();

                var pdfBytes = memoryStream.ToArray();
                return File(pdfBytes, "application/pdf", "DanhSachNhanVien.pdf");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server khi xuất PDF: {ex.Message}");
        }
    }
    [HttpGet("check-cccd")]
    public async Task<IActionResult> CheckCccd([FromQuery] string cccd)
    {
        try
        {
            var exists = await _staffRepository.IsCccdExistsAsync(cccd);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpGet("check-sdt")]
    public async Task<IActionResult> CheckSdt([FromQuery] string sdt)
    {
        try
        {
            var exists = await _staffRepository.IsSdtExistsAsync(sdt);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }
    [HttpGet("check-ten-tai-khoan")]
    public async Task<IActionResult> CheckTenTaiKhoan([FromQuery] string tenTaiKhoan)
    {
        try
        {
            var exists = await _staffRepository.IsTenTaiKhoanExistsAsync(tenTaiKhoan);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        try
        {
            var exists = await _staffRepository.IsEmailExistsAsync(email);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportStaffToExcel()
    {
        try
        {
            var staffList = await _staffRepository.GetStaffForExportAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Nhân Viên");

                worksheet.Cell(1, 1).Value = "Họ Tên";
                worksheet.Cell(1, 2).Value = "Giới Tính";
                worksheet.Cell(1, 3).Value = "Ngày Sinh";
                worksheet.Cell(1, 4).Value = "Địa Chỉ";
                worksheet.Cell(1, 5).Value = "SĐT";
                worksheet.Cell(1, 6).Value = "Email";
                worksheet.Cell(1, 7).Value = "Ngày Vào Làm";
                worksheet.Cell(1, 8).Value = "Tình Trạng";
                worksheet.Cell(1, 9).Value = "Chức Vụ";

                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;
                foreach (var staff in staffList)
                {
                    worksheet.Cell(row, 1).Value = staff.HoTen ?? "";
                    worksheet.Cell(row, 2).Value = staff.GioiTinh ?? "";
                    worksheet.Cell(row, 3).Value = staff.NgaySinh == DateTime.MinValue ? "" : staff.NgaySinh.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 4).Value = staff.DiaChi ?? "";
                    worksheet.Cell(row, 5).Value = staff.SDT ?? "";
                    worksheet.Cell(row, 6).Value = staff.Email ?? "";
                    worksheet.Cell(row, 7).Value = staff.NgayVaoLam.HasValue ? staff.NgayVaoLam.Value.ToString("dd/MM/yyyy") : "";
                    worksheet.Cell(row, 8).Value = staff.TinhTrang ?? "";
                    worksheet.Cell(row, 9).Value = staff.TenChucVu ?? "";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var tableRange = worksheet.Range(1, 1, row - 1, 9);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Position = 0;

                    return File(
                        memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "DanhSachNhanVien.xlsx");
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server khi xuất Excel: {ex.Message}");
        }
    }

    [HttpGet("image/{fileName}")]
    public IActionResult GetStaffImage(string fileName)
    {
        try
        {
            // Xây dựng đường dẫn đến hình ảnh
            var imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "AnhNhanVien", fileName);

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