using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/inventories")]
    public class InventoriesController : ControllerBase
    {
        private readonly InventoryService _inventoryService;
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public InventoriesController(CargoHubDbContext context, InventoryService inventoryService, ILoggingService loggingService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all inventories", Description = "Returns a list of inventories with optional limit.")]
        [SwaggerResponse(200, "List of inventories", typeof(IEnumerable<Inventory>))]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _inventoryService.GetInventoriesAsync(limit.Value)
                : await _inventoryService.GetAllInventoriesAsync();
            return Ok(entities);
        }

        // Voorraad per locatie tonen
        [HttpGet("{inventoryId:int}/locations")]
        [SwaggerOperation(Summary = "Get inventory locations", Description = "Returns a list of inventory locations for a specific inventory.")]
        [SwaggerResponse(200, "List of inventory locations", typeof(IEnumerable<InventoryLocation>))]
        public async Task<IActionResult> GetInventoryLocations(int inventoryId)
        {
            var inventoryLocations = await _context.InventoryLocations
                .Include(il => il.Location)
                .Where(il => il.InventoryId == inventoryId)
                .ToListAsync();

            return Ok(inventoryLocations);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get inventory by ID", Description = "Returns a single inventory by its unique identifier.")]
        [SwaggerResponse(200, "Inventory found", typeof(Inventory))]
        public async Task<ActionResult<Inventory>> GetById(int id)
        {
            var entity = await _inventoryService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new inventory", Description = "Adds a new inventory to the system.")]
        [SwaggerResponse(201, "Inventory created", typeof(Inventory))]
        public async Task<ActionResult<Inventory>> Create([FromBody] Inventory inventory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _inventoryService.AddInventoryAsync(inventory);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Inventory",
                "Create",
                HttpContext.Request.Path,
                $"Inventory created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Update an existing inventory", Description = "Updates the details of an existing inventory.")]
        [SwaggerResponse(200, "Inventory updated", typeof(Inventory))]
        public async Task<ActionResult<Inventory>> Update(int id, [FromBody] Inventory updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.UpdateInventoryAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Inventory",
                "Update",
                HttpContext.Request.Path,
                $"Inventory {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Delete an inventory", Description = "Deletes an existing inventory by its unique identifier.")]
        [SwaggerResponse(204, "Inventory deleted")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _inventoryService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Inventory",
                "Delete",
                HttpContext.Request.Path,
                $"Inventory {id} soft-deleted");

            return NoContent();
        }
    }
}

