using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/itemgroups")]
    public class ItemGroupController : ControllerBase
    {
        private readonly ItemGroupService _itemGroupService;
        private readonly ILoggingService _loggingService;

        public ItemGroupController(ItemGroupService itemGroupService, ILoggingService loggingService)
        {
            _itemGroupService = itemGroupService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item_Group>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemGroupService.GetItemGroupsAsync(limit.Value)
                : await _itemGroupService.GetAllItemGroupsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Item_Group>> GetById(int id)
        {
            var entity = await _itemGroupService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Item_Group>> Create([FromBody] Item_Group itemGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _itemGroupService.AddItemGroupAsync(itemGroup);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemGroup",
                "Create",
                HttpContext.Request.Path,
                $"ItemGroup created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Item_Group>> Update(int id, [FromBody] Item_Group updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itemGroupService.UpdateItemGroupAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemGroup",
                "Update",
                HttpContext.Request.Path,
                $"ItemGroup {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _itemGroupService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemGroup",
                "Delete",
                HttpContext.Request.Path,
                $"ItemGroup {id} soft-deleted");

            return NoContent();
        }
    }
}

