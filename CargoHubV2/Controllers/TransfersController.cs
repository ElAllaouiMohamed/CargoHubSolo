using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/transfers")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly ILoggingService _loggingService;

        public TransfersController(ITransferService transferService, ILoggingService loggingService)
        {
            _transferService = transferService;
            _loggingService = loggingService;
        }




        [HttpGet]
        [SwaggerOperation(Summary = "Get all transfers", Description = "Returns a list of transfers with optional limit.")]
        [SwaggerResponse(200, "List of transfers", typeof(IEnumerable<Transfer>))]
        public async Task<ActionResult<IEnumerable<Transfer>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _transferService.GetTransfersAsync(limit.Value)
                : await _transferService.GetAllTransfersAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get transfer by ID", Description = "Returns a single transfer by its unique identifier.")]
        [SwaggerResponse(200, "Transfer found", typeof(Transfer))]
        public async Task<ActionResult<Transfer>> GetById(int id)
        {
            var entity = await _transferService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new transfer", Description = "Adds a new transfer to the system.")]
        [SwaggerResponse(201, "Transfer created successfully", typeof(Transfer))]
        public async Task<ActionResult<Transfer>> Create([FromBody] Transfer transfer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _transferService.AddTransferAsync(transfer);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Transfer",
                "Create",
                HttpContext.Request.Path,
                $"Transfer created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Update an existing transfer", Description = "Updates the details of a transfer by its unique identifier.")]
        [SwaggerResponse(200, "Transfer updated successfully", typeof(Transfer))]
        public async Task<ActionResult<Transfer>> Update(int id, [FromBody] Transfer updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transferService.UpdateTransferAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Transfer",
                "Update",
                HttpContext.Request.Path,
                $"Transfer {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Soft delete a transfer", Description = "Marks a transfer as deleted without removing it from the database.")]
        [SwaggerResponse(204, "Transfer soft-deleted")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _transferService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Transfer",
                "Delete",
                HttpContext.Request.Path,
                $"Transfer {id} soft-deleted");

            return NoContent();
        }
    }
}

