using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly SupplierService _supplierService;
        private readonly ILoggingService _loggingService;

        public SupplierController(SupplierService supplierService, ILoggingService loggingService)
        {
            _supplierService = supplierService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _supplierService.GetSuppliersAsync(limit.Value)
                : await _supplierService.GetAllSuppliersAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Supplier>> GetById(int id)
        {
            var entity = await _supplierService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Supplier>> Create([FromBody] Supplier supplier)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _supplierService.AddSupplierAsync(supplier);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Supplier",
                "Create",
                HttpContext.Request.Path,
                $"Supplier created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Supplier>> Update(int id, [FromBody] Supplier updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _supplierService.UpdateSupplierAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Supplier",
                "Update",
                HttpContext.Request.Path,
                $"Supplier {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _supplierService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Supplier",
                "Delete",
                HttpContext.Request.Path,
                $"Supplier {id} soft-deleted");

            return NoContent();
        }
    }
}
