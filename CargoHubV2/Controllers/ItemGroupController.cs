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

        public ItemGroupController(ItemGroupService itemGroupService)
        {
            _itemGroupService = itemGroupService;
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

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _itemGroupService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
