using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/items")]
    public class ItemsController : ControllerBase
    {
        private readonly ItemService _itemService;

        public ItemsController(ItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemService.GetItemsAsync(limit.Value)
                : await _itemService.GetAllItemsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Item>> GetById(int id)
        {
            var entity = await _itemService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Item>> Create([FromBody] Item item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _itemService.AddItemAsync(item);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Item>> Update(int id, [FromBody] Item updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itemService.UpdateItemAsync(id, updated);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _itemService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
