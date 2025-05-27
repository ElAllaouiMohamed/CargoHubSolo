using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/itemtypes")]
    public class ItemTypeController : ControllerBase
    {
        private readonly ItemTypeService _itemTypeService;
        private readonly ILoggingService _loggingService;

        public ItemTypeController(ItemTypeService itemTypeService, ILoggingService loggingService)
        {
            _itemTypeService = itemTypeService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all item types", Description = "Returns a list of item types with optional limit.")]
        [SwaggerResponse(200, "List of item types", typeof(IEnumerable<Item_Type>))]
        public async Task<ActionResult<IEnumerable<Item_Type>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemTypeService.GetItemTypesAsync(limit.Value)
                : await _itemTypeService.GetAllItemTypesAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get item type by ID", Description = "Returns a single item type by its unique identifier.")]
        [SwaggerResponse(200, "Item type found", typeof(Item_Type))]
        public async Task<ActionResult<Item_Type>> GetById(int id)
        {
            var entity = await _itemTypeService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new item type", Description = "Adds a new item type to the system.")]
        [SwaggerResponse(201, "Item type created", typeof(Item_Type))]
        public async Task<ActionResult<Item_Type>> Create([FromBody] Item_Type itemType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _itemTypeService.AddItemTypeAsync(itemType);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemType",
                "Create",
                HttpContext.Request.Path,
                $"ItemType created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Update an existing item type", Description = "Updates an existing item type in the system.")]
        [SwaggerResponse(200, "Item type updated", typeof(Item_Type))]
        public async Task<ActionResult<Item_Type>> Update(int id, [FromBody] Item_Type updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itemTypeService.UpdateItemTypeAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemType",
                "Update",
                HttpContext.Request.Path,
                $"ItemType {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Delete an item type", Description = "Deletes an existing item type by its unique identifier.")]
        [SwaggerResponse(204, "Item type deleted")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _itemTypeService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemType",
                "Delete",
                HttpContext.Request.Path,
                $"ItemType {id} soft-deleted");

            return NoContent();
        }
    }
}

