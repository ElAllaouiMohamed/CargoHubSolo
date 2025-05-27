using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/shipments")]
    public class ShipmentsController : ControllerBase
    {
        private readonly ShipmentService _shipmentService;
        private readonly ILoggingService _loggingService;

        public ShipmentsController(ShipmentService shipmentService, ILoggingService loggingService)
        {
            _shipmentService = shipmentService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _shipmentService.GetShipmentsAsync(limit.Value)
                : await _shipmentService.GetAllShipmentsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Shipment>> GetById(int id)
        {
            var entity = await _shipmentService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Shipment>> Create([FromBody] Shipment shipment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _shipmentService.AddShipmentAsync(shipment);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Shipment",
                "Create",
                HttpContext.Request.Path,
                $"Shipment created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Shipment>> Update(int id, [FromBody] Shipment updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _shipmentService.UpdateShipmentAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Shipment",
                "Update",
                HttpContext.Request.Path,
                $"Shipment {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _shipmentService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Shipment",
                "Delete",
                HttpContext.Request.Path,
                $"Shipment {id} soft-deleted");

            return NoContent();
        }
    }
}

