using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/itemgroups")]
    public class ItemGroupController : ControllerBase
    {
        private readonly IItemGroupService _itemGroupService;
        private readonly ILoggingService _loggingService;

        public ItemGroupController(IItemGroupService itemGroupService, ILoggingService loggingService)
        {
            _itemGroupService = itemGroupService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all item groups", Description = "Returns a list of item groups with optional limit.")]
        [SwaggerResponse(200, "List of item groups", typeof(IEnumerable<Item_Group>))]
        public async Task<ActionResult<IEnumerable<Item_Group>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemGroupService.GetItemGroupsAsync(limit.Value)
                : await _itemGroupService.GetAllItemGroupsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get item group by ID", Description = "Returns a single item group by its unique identifier.")]
        [SwaggerResponse(200, "Item group found", typeof(Item_Group))]
        public async Task<ActionResult<Item_Group>> GetById(int id)
        {
            var entity = await _itemGroupService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new item group", Description = "Adds a new item group to the system.")]
        [SwaggerResponse(201, "Item group created", typeof(Item_Group))]
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
        [SwaggerOperation(Summary = "Update an existing item group", Description = "Updates the details of an existing item group.")]
        [SwaggerResponse(200, "Item group updated", typeof(Item_Group))]
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
        [SwaggerOperation(Summary = "Soft delete an item group", Description = "Marks an item group as deleted without removing it from the database.")]
        [SwaggerResponse(204, "Item group soft-deleted")]
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

