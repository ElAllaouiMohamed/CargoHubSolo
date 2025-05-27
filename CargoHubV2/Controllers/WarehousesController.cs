using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/warehouses")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILoggingService _loggingService;

        public WarehousesController(IWarehouseService warehouseService, ILoggingService loggingService)
        {
            _warehouseService = warehouseService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all warehouses", Description = "Returns a list of warehouses with optional limit.")]
        [SwaggerResponse(200, "List of warehouses", typeof(IEnumerable<Warehouse>))]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _warehouseService.GetWarehousesAsync(limit.Value)
                : await _warehouseService.GetAllWarehousesAsync();
            return Ok(entities);
        }

        [HttpGet("{warehouseId}/hazard-check")]
        [SwaggerOperation(Summary = "Check hazard compliance", Description = "Checks if the items in the warehouse comply with its hazard classification.")]
        [SwaggerResponse(200, "No forbidden items found")]
        [SwaggerResponse(400, "Forbidden items detected")]
        [SwaggerResponse(404, "Warehouse not found")]
        public async Task<IActionResult> CheckHazardCompliance(int warehouseId)
        {
            var (isCompliant, forbiddenItems) = await _warehouseService.CheckHazardComplianceAsync(warehouseId);

            if (forbiddenItems == null)
                return NotFound();

            if (!isCompliant)
            {
                return BadRequest(new { message = "Forbidden items detected", items = forbiddenItems });
            }

            return Ok(new { message = "No forbidden items found" });
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get warehouse by ID", Description = "Returns a single warehouse by its unique identifier.")]
        [SwaggerResponse(200, "Warehouse found", typeof(Warehouse))]
        public async Task<ActionResult<Warehouse>> GetById(int id)
        {
            var entity = await _warehouseService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new warehouse", Description = "Adds a new warehouse to the system.")]
        [SwaggerResponse(201, "Warehouse created successfully", typeof(Warehouse))]
        public async Task<ActionResult<Warehouse>> Create([FromBody] Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _warehouseService.AddWarehouseAsync(warehouse);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Warehouse",
                "Create",
                HttpContext.Request.Path,
                $"Warehouse created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Update an existing warehouse", Description = "Updates the details of an existing warehouse.")]
        [SwaggerResponse(200, "Warehouse updated successfully", typeof(Warehouse))]
        public async Task<ActionResult<Warehouse>> Update(int id, [FromBody] Warehouse updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _warehouseService.UpdateWarehouseAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Warehouse",
                "Update",
                HttpContext.Request.Path,
                $"Warehouse {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Soft delete a warehouse", Description = "Marks a warehouse as deleted without removing it from the database.")]
        [SwaggerResponse(204, "Warehouse soft-deleted")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _warehouseService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Warehouse",
                "Delete",
                HttpContext.Request.Path,
                $"Warehouse {id} soft-deleted");

            return NoContent();
        }
    }
}
