using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/reporting")]
    public class ReportingController : ControllerBase
    {
        private readonly ReportingService _reportingService;

        public ReportingController(ReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("warehouse/{warehouseId}")]
        public IActionResult GetWarehouseReport(int warehouseId)
        {
            var report = _reportingService.GetWarehouseReport(warehouseId);
            return Ok(report);
        }

        [HttpGet("orders-csv/{warehouseId}")]
        public IActionResult GetOrdersCsv(int warehouseId)
        {
            var csv = _reportingService.GenerateCsvReport(warehouseId);
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"orders_warehouse_{warehouseId}.csv");
        }

        [HttpGet("revenue")]
        public IActionResult GetRevenue([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var revenue = _reportingService.GetRevenueBetweenDates(start, end);
            return Ok(revenue);
        }
    }
}

