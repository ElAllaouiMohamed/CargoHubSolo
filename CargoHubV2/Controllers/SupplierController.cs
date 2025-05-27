using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILoggingService _loggingService;

        public SupplierController(ISupplierService supplierService, ILoggingService loggingService)
        {
            _supplierService = supplierService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all suppliers", Description = "Returns a list of suppliers with optional limit.")]
        [SwaggerResponse(200, "List of suppliers", typeof(IEnumerable<Supplier>))]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _supplierService.GetSuppliersAsync(limit.Value)
                : await _supplierService.GetAllSuppliersAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get supplier by ID", Description = "Returns a single supplier by its unique identifier.")]
        [SwaggerResponse(200, "Supplier found", typeof(Supplier))]
        public async Task<ActionResult<Supplier>> GetById(int id)
        {
            var entity = await _supplierService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new supplier", Description = "Adds a new supplier to the system.")]
        [SwaggerResponse(201, "Supplier created successfully", typeof(Supplier))]
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

        [SwaggerOperation(Summary = "Update a supplier", Description = "Updates an existing supplier by its unique identifier.")]
        [SwaggerResponse(200, "Supplier updated successfully", typeof(Supplier))]
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
        [SwaggerOperation(Summary = "Delete a supplier", Description = "Deletes an existing supplier by its unique identifier.")]
        [SwaggerResponse(204, "Supplier deleted successfully")]
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

