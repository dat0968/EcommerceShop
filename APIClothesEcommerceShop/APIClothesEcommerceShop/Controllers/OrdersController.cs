using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Nhân viên")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        public OrdersController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? filter, int page = 1)
        {
            try
            {
                var pagesize = 10;
                page = page < 1 ? 1 : page;
                var orders = await orderRepository.GetAll(search, filter);
                // Sắp xếp theo maHd giảm dần để mã hóa đơn cao nhất lên đầu
                orders = orders.OrderByDescending(o => o.MaHd).ToList();
                var PagedOrders = orders.Skip((page - 1) * pagesize).Take(pagesize);
                var ToTalPage = Math.Ceiling((decimal)orders.Count() / pagesize);
                return Ok(new
                {
                    Data = PagedOrders,
                    ToTalPage = ToTalPage,
                    Page = page,
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, UpdateOrderDTO model)
        {
            try
            {
                await orderRepository.UpdateStatusOrders(id, model.Status, model.MaNv, model.PaymentMethod, model.ReasonCancel);
                return Ok(new
                {
                    Success = true,
                    Message = "Cập nhật trạng thái đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        [HttpGet("xuat-pdf/{maHd}")]
        public async Task<IActionResult> XuatHoaDonPdf(int maHd)
        {
            var order = await orderRepository.GetbyId(maHd);
            if (order == null) return NotFound();

            var document = new HoaDonDocument(order);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"HoaDon_{maHd}.pdf");
        }
    }
}