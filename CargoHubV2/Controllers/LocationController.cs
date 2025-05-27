using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerOperation(Summary = "Get all locations", Description = "Returns a list of locations with optional limit.")] 
        [SwaggerResponse(200, "List of locations", typeof(IEnumerable<Location>))]
        public async Task<ActionResult<IEnumerable<Location>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _locationService.GetLocationsAsync(limit.Value)
                : await _locationService.GetAllLocationsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get location by ID", Description = "Returns a single location by its unique identifier.")]
        [SwaggerResponse(200, "Location found", typeof(Location))]
        public async Task<ActionResult<Location>> GetById(int id)
        {
            var entity = await _locationService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost] 
        [SwaggerOperation(Summary = "Create a new location", Description = "Adds a new location to the system.")]
        [SwaggerResponse(201, "Location created", typeof(Location))]
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
        [SwaggerOperation(Summary = "Update a location", Description = "Updates an existing location by its unique identifier.")]
        [SwaggerResponse(200, "Location updated", typeof(Location))]
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
        [SwaggerOperation(Summary = "Soft delete a location", Description = "Marks a location as deleted without removing it from the database.")]
        [SwaggerResponse(204, "Location soft-deleted")]
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

