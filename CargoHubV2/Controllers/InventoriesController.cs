using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/inventories")]
    public class InventoriesController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoriesController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _inventoryService.GetInventoriesAsync(limit.Value)
                : await _inventoryService.GetAllInventoriesAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Inventory>> GetById(int id)
        {
            var entity = await _inventoryService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Inventory>> Create([FromBody] Inventory inventory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _inventoryService.AddInventoryAsync(inventory);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Inventory>> Update(int id, [FromBody] Inventory updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.UpdateInventoryAsync(id, updated);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _inventoryService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
