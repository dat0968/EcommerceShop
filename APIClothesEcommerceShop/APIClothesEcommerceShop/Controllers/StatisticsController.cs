using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Statistics;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticRepository _statisticRepository;
        public StatisticsController(IStatisticRepository statisticRepository)
        {
            _statisticRepository = statisticRepository;
        }
        /// <summary>
        ///     Nhận số liệu thống kê sản phẩm
        /// </summary>
        /// <returns>
        ///     Trả về thống kê sản phẩm
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<ProductStatisticsResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetProductStatistics()
        {
            try
            {
                var result = await _statisticRepository.GetProductStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        ///     Nhận số liệu thống kê của khách hàng
        /// </summary>
        /// <returns>
        ///     Số liệu thống kê của khách hàng
        ///     <para>Trả về danh sách khách hàng</para>
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<CustomerStatisticsResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerStatistics()
        {
            var result = await _statisticRepository.GetCustomerStatisticsAsync();
            return Ok(result);
        }
        /// <summary>
        ///     Nhận số liệu thống kê của nhân viên
        /// </summary>
        /// <returns>
        ///     Trả về danh sách thống kê nhân viên
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<EmployeeStatisticsResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeStatistics()
        {
            var result = await _statisticRepository.GetEmployeeStatisticsAsync();
            return Ok(result);
        }
        /// <summary>
        ///     Nhận số liệu thống kê doanh thu
        /// </summary>
        /// <returns>
        ///     Trả về thống kê doanh thu
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<RevenueStatisticsResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetRevenueStatistics()
        {
            var result = await _statisticRepository.GetRevenueStatisticsAsync();
            return Ok(result);
        }
        /// <summary>
        ///     Nhận tổng hợp đơn hàng
        /// </summary>
        /// <returns>
        ///     Trả về thống kê tổng hợp đơn hàng
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<OrderSummaryResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetOrderSummary()
        {
            var result = await _statisticRepository.GetOrderSummaryByOrder();
            return Ok(result);
        }
        /// <summary>
        ///     Nhận số liệu thống kê combo sản phẩm
        /// </summary>
        /// <returns>
        ///     Trả về thống kê combo sản phẩm
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<ComboStatisticsResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetComboStatistics()
        {
            var result = await _statisticRepository.GetComboStatisticsAsync();
            return Ok(result);
        }
        /// <summary>
        ///     Nhận số liệu thống kê cho datatable
        /// </summary>
        /// <returns>
        ///     Trả về thống kê cho datatable
        /// </returns>
        [ProducesResponseType(typeof(ResponseAPI<DatatableStatisticsResponse>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetDatatableStatistics()
        {
            var result = await _statisticRepository.GetDatatableStatisticsAsync();
            return Ok(result);
        }
    }
}