using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/itemlines")]
    public class ItemLineController : ControllerBase
    {
        private readonly ItemLineService _itemLineService;
        private readonly ILoggingService _loggingService;

        public ItemLineController(ItemLineService itemLineService, ILoggingService loggingService)
        {
            _itemLineService = itemLineService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item_Line>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemLineService.GetItemLinesAsync(limit.Value)
                : await _itemLineService.GetAllItemLinesAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Item_Line>> GetById(int id)
        {
            var entity = await _itemLineService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Item_Line>> Create([FromBody] Item_Line itemLine)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _itemLineService.AddItemLineAsync(itemLine);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemLine",
                "Create",
                HttpContext.Request.Path,
                $"ItemLine created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Item_Line>> Update(int id, [FromBody] Item_Line updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itemLineService.UpdateItemLineAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemLine",
                "Update",
                HttpContext.Request.Path,
                $"ItemLine {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _itemLineService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ItemLine",
                "Delete",
                HttpContext.Request.Path,
                $"ItemLine {id} soft-deleted");

            return NoContent();
        }
    }
}

