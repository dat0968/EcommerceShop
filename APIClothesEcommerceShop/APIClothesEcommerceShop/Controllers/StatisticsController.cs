using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.Statistics;
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
        [HttpGet]
        public async Task<IActionResult> GetCustomerStatistics()
        {
            var result = await _statisticRepository.GetCustomerStatisticsAsync();
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeeStatistics()
        {
            var result = await _statisticRepository.GetEmployeeStatisticsAsync();
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetRevenueStatistics()
        {
            var result = await _statisticRepository.GetRevenueStatisticsAsync();
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderSummary()
        {
            var result = await _statisticRepository.GetOrderSummaryByOrder();
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetComboStatistics()
        {
            var result = await _statisticRepository.GetComboStatisticsAsync();
            return Ok(result);
        }
    }
}