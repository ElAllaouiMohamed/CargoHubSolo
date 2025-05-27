using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerOperation(Summary = "Get all shipments", Description = "Returns a list of shipments with optional limit.")]
        [SwaggerResponse(200, "List of shipments", typeof(IEnumerable<Shipment>))]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _shipmentService.GetShipmentsAsync(limit.Value)
                : await _shipmentService.GetAllShipmentsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get shipment by ID", Description = "Returns a single shipment by its unique identifier.")]
        [SwaggerResponse(200, "Shipment found", typeof(Shipment))]
        public async Task<ActionResult<Shipment>> GetById(int id)
        {
            var entity = await _shipmentService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new shipment", Description = "Adds a new shipment to the system.")]
        [SwaggerResponse(201, "Shipment created successfully", typeof(Shipment))]
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
        [SwaggerOperation(Summary = "Update an existing shipment", Description = "Updates the details of an existing shipment by its unique identifier.")]
        [SwaggerResponse(200, "Shipment updated successfully", typeof(Shipment))]
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
        [SwaggerOperation(Summary = "Delete a shipment", Description = "Deletes an existing shipment by its unique identifier.")]
        [SwaggerResponse(204, "Shipment deleted successfully")]
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

