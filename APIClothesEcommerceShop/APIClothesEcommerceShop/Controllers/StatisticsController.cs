using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<IActionResult> GetOrderStatistics()
        {
            var result = await _statisticRepository.GetOrderStatisticsAsync();
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetProductStatistics()
        {
            var result = await _statisticRepository.GetProductStatisticsAsync();
            return Ok(result);
        }
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
        public async Task<IActionResult> GetOrderSummaryBy()
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