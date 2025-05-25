using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/warehouses")]
    public class WarehousesController : ControllerBase
    {
        private readonly WarehouseService _warehouseService;

        public WarehousesController(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _warehouseService.GetWarehousesAsync(limit.Value)
                : await _warehouseService.GetAllWarehousesAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Warehouse>> GetById(int id)
        {
            var entity = await _warehouseService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Warehouse>> Create([FromBody] Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _warehouseService.AddWarehouseAsync(warehouse);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Warehouse>> Update(int id, [FromBody] Warehouse updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _warehouseService.UpdateWarehouseAsync(id, updated);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _warehouseService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
