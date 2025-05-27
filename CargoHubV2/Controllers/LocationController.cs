using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/locations")]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;
        private readonly ILoggingService _loggingService;

        public LocationController(LocationService locationService, ILoggingService loggingService)
        {
            _locationService = locationService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _locationService.GetLocationsAsync(limit.Value)
                : await _locationService.GetAllLocationsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Location>> GetById(int id)
        {
            var entity = await _locationService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Location>> Create([FromBody] Location location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _locationService.AddLocationAsync(location);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Location",
                "Create",
                HttpContext.Request.Path,
                $"Location created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Location>> Update(int id, [FromBody] Location updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _locationService.UpdateLocationAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Location",
                "Update",
                HttpContext.Request.Path,
                $"Location {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _locationService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Location",
                "Delete",
                HttpContext.Request.Path,
                $"Location {id} soft-deleted");

            return NoContent();
        }
    }
}

