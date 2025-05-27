using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/items")]
    public class ItemsController : ControllerBase
    {
        private readonly ItemService _itemService;
        private readonly ILoggingService _loggingService;

        public ItemsController(ItemService itemService, ILoggingService loggingService)
        {
            _itemService = itemService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all items", Description = "Returns a list of items with optional limit.")]
        [SwaggerResponse(200, "List of items", typeof(IEnumerable<Item>))]
        public async Task<ActionResult<IEnumerable<Item>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemService.GetItemsAsync(limit.Value)
                : await _itemService.GetAllItemsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get item by ID", Description = "Returns a single item by its unique identifier.")]
        [SwaggerResponse(200, "Item found", typeof(Item))]
        public async Task<ActionResult<Item>> GetById(int id)
        {
            var entity = await _itemService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new item", Description = "Adds a new item to the system.")]
        [SwaggerResponse(201, "Item created", typeof(Item))]
        public async Task<ActionResult<Item>> Create([FromBody] Item item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _itemService.AddItemAsync(item);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Item",
                "Create",
                HttpContext.Request.Path,
                $"Item created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Update an existing item", Description = "Updates an existing item in the system.")]
        [SwaggerResponse(200, "Item updated", typeof(Item))]
        public async Task<ActionResult<Item>> Update(int id, [FromBody] Item updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itemService.UpdateItemAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Item",
                "Update",
                HttpContext.Request.Path,
                $"Item {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Delete an item", Description = "Deletes an existing item by its unique identifier.")]
        [SwaggerResponse(204, "Item deleted")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _itemService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Item",
                "Delete",
                HttpContext.Request.Path,
                $"Item {id} soft-deleted");

            return NoContent();
        }
    }
}

