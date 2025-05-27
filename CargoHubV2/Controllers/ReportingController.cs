using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerOperation(Summary = "Get warehouse report", Description = "Generates a report for the specified warehouse.")]
        [SwaggerResponse(200, "Warehouse report", typeof(object))]
        public IActionResult GetWarehouseReport(int warehouseId)
        {
            var report = _reportingService.GetWarehouseReport(warehouseId);
            return Ok(report);
        }

        [HttpGet("orders-csv/{warehouseId}")]
        [SwaggerOperation(Summary = "Get orders CSV report", Description = "Generates a CSV report of orders for the specified warehouse.")]
        [SwaggerResponse(200, "CSV file of orders", typeof(FileResult))]
        public IActionResult GetOrdersCsv(int warehouseId)
        {
            var csv = _reportingService.GenerateCsvReport(warehouseId);
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"orders_warehouse_{warehouseId}.csv");
        }

        [HttpGet("revenue")]
        [SwaggerOperation(Summary = "Get revenue between dates", Description = "Returns the total revenue generated between the specified start and end dates.")]
        [SwaggerResponse(200, "Total revenue", typeof(decimal))]
        public IActionResult GetRevenue([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var revenue = _reportingService.GetRevenueBetweenDates(start, end);
            return Ok(revenue);
        }
    }
}

