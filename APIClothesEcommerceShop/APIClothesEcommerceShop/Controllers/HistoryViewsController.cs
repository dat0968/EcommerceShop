using APIClothesEcommerceShop.Repositories.ViewHistory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryViewsController : ControllerBase
    {
        private readonly IViewHistoryRepository _lichSuXemRepo;

        public HistoryViewsController(IViewHistoryRepository lichSuXemRepo)
        {
            _lichSuXemRepo = lichSuXemRepo;
        }

        [HttpPost("xem-san-pham")]
        public async Task<IActionResult> XemSanPham(int maNguoiDung, int? maSanPham, int? maCombo)
        {
            await _lichSuXemRepo.AddOrUpdateAsync(maNguoiDung, maSanPham, maCombo);
            return Ok(new { success = true });
        }

        [HttpGet("lich-su/{maNguoiDung}")]
        public async Task<IActionResult> LayLichSu(int maNguoiDung)
        {
            var lichSu = await _lichSuXemRepo.getHistoryAsync(maNguoiDung);
            return Ok(lichSu);
        }
    }
}
