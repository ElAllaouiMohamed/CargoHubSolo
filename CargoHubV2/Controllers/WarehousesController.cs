using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/warehouses")]
    public class WarehousesController : ControllerBase
    {
        private readonly WarehouseService _warehouseService;
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public WarehousesController(CargoHubDbContext context, WarehouseService warehouseService, ILoggingService loggingService)
        {
            _context = context;
            _warehouseService = warehouseService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _warehouseService.GetWarehousesAsync(limit.Value)
                : await _warehouseService.GetAllWarehousesAsync();
            return Ok(entities);
        }

        [HttpGet("{warehouseId}/hazard-check")]
        public async Task<IActionResult> CheckHazardCompliance(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse == null)
                return NotFound();

            var forbiddenItems = await _context.Inventories
                .Where(inv => inv.InventoryLocations.Any(il => il.Location.WarehouseId == warehouseId)
                            && (int)inv.HazardClassification > (int)warehouse.HazardClassification)
                .ToListAsync();

            if (forbiddenItems.Any())
            {
                return BadRequest(new { message = "Forbidden items detected", items = forbiddenItems });
            }

            return Ok(new { message = "No forbidden items found" });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Warehouse>> GetById(int id)
        {
            var entity = await _warehouseService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
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
