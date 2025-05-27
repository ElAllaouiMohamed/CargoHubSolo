using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<ActionResult<IEnumerable<Item_Type>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemTypeService.GetItemTypesAsync(limit.Value)
                : await _itemTypeService.GetAllItemTypesAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Item_Type>> GetById(int id)
        {
            var entity = await _itemTypeService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
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

