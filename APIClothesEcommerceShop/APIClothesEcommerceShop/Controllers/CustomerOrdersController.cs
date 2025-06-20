using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Repositories.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        public CustomerOrdersController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }
        [HttpGet("{maKh}")]
        public async Task<IActionResult> Index([FromRoute] int maKh, string? search, string? filter, int page)
        {
            try
            {
                var pagesize = 10;
                page = page < 1 ? 1 : page;
                var listOrder = await orderRepository.GetByMakh(maKh, search, filter);
                var PagedOrders = listOrder.Skip((page - 1) * pagesize).Take(pagesize);
                var ToTalPage = Math.Ceiling((decimal)listOrder.Count() / pagesize);
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
        [HttpPost]
        public async Task<IActionResult> CancelOrders([FromBody] CancelOrderRequestDTO request)
        {
            try
            {
                await orderRepository.CancelOrders(request.Id, request.SelectedCancelStatus, request.ReasonCancel);
                return Ok(new
                {
                    Success = true,
                    Message = "Đã hủy/hoàn trả đơn hàng"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
