using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

using Swashbuckle.AspNetCore.Annotations;


namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/itemlines")]
    public class ItemLineController : ControllerBase
    {
        private readonly IItemLineService _itemLineService;
        private readonly ILoggingService _loggingService;

        public ItemLineController(IItemLineService itemLineService, ILoggingService loggingService)
        {
            _itemLineService = itemLineService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all item lines", Description = "Returns a list of item lines with optional limit.")]
        [SwaggerResponse(200, "List of item lines", typeof(IEnumerable<Item_Line>))]
        public async Task<ActionResult<IEnumerable<Item_Line>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _itemLineService.GetItemLinesAsync(limit.Value)
                : await _itemLineService.GetAllItemLinesAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get item line by ID", Description = "Returns a single item line by its unique identifier.")]
        [SwaggerResponse(200, "Item line found", typeof(Item_Line))]
        public async Task<ActionResult<Item_Line>> GetById(int id)
        {
            var entity = await _itemLineService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new item line", Description = "Adds a new item line to the system.")]
        [SwaggerResponse(201, "Item line created", typeof(Item_Line))]
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
        [SwaggerOperation(Summary = "Update an existing item line", Description = "Updates an existing item line by its unique identifier.")]
        [SwaggerResponse(200, "Item line updated", typeof(Item_Line))]
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
        [SwaggerOperation(Summary = "Delete an item line", Description = "Deletes an existing item line by its unique identifier.")]
        [SwaggerResponse(204, "Item line deleted")]
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

