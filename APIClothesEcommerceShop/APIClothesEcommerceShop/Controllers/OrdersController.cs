using APIClothesEcommerceShop.Repositories.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
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
        [HttpPut]
        public async Task<IActionResult> Update(int id, string status, int MaNv, string paymentmethod, string? reasonCancel)
        {
            try
            {
                await orderRepository.UpdateStatusOrders(id, status, MaNv, paymentmethod, reasonCancel);
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
    }
}
