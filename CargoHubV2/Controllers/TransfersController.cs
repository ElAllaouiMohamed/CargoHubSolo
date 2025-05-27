using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/transfers")]
    public class TransfersController : ControllerBase
    {
        private readonly TransferService _transferService;
        private readonly ILoggingService _loggingService;

        public TransfersController(TransferService transferService, ILoggingService loggingService)
        {
            _transferService = transferService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transfer>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _transferService.GetTransfersAsync(limit.Value)
                : await _transferService.GetAllTransfersAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Transfer>> GetById(int id)
        {
            var entity = await _transferService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
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

