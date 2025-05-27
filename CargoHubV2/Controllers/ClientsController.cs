using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly ClientService _clientService;
        private readonly ILoggingService _loggingService;

        public ClientsController(ClientService clientService, ILoggingService loggingService)
        {
            _clientService = clientService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _clientService.GetClientsAsync(limit.Value)
                : await _clientService.GetAllClientsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Client>> GetById(int id)
        {
            var entity = await _clientService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // Contactpersonen ophalen voor client
        [HttpGet("{clientId:int}/contactpersons")]
        public async Task<ActionResult<IEnumerable<ContactPerson>>> GetContactPersons(int clientId)
        {
            var contactPersons = await _clientService.GetContactPersonsByClientIdAsync(clientId);
            return Ok(contactPersons);
        }

        // Contactpersoon toevoegen aan client
        [HttpPost("{clientId:int}/contactpersons")]
        public async Task<ActionResult<ContactPerson>> AddContactPerson(int clientId, [FromBody] ContactPerson contactPerson)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var added = await _clientService.AddContactPersonToClientAsync(clientId, contactPerson);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ContactPerson",
                "Create",
                HttpContext.Request.Path,
                $"ContactPerson added to Client {clientId}, ContactPersonId: {added.Id}");

            return CreatedAtAction(nameof(GetContactPersons), new { clientId }, added);
        }

        // Contactpersoon bijwerken
        [HttpPut("contactpersons/{contactPersonId:int}")]
        public async Task<ActionResult<ContactPerson>> UpdateContactPerson(int contactPersonId, [FromBody] ContactPerson updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _clientService.UpdateContactPersonAsync(contactPersonId, updated);

            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ContactPerson",
                "Update",
                HttpContext.Request.Path,
                $"ContactPerson {contactPersonId} updated");

            return Ok(result);
        }

        // Contactpersoon verwijderen
        [HttpDelete("contactpersons/{contactPersonId:int}")]
        public async Task<IActionResult> DeleteContactPerson(int contactPersonId)
        {
            var success = await _clientService.DeleteContactPersonAsync(contactPersonId);
            if (!success)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "ContactPerson",
                "Delete",
                HttpContext.Request.Path,
                $"ContactPerson {contactPersonId} deleted");

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Client>> Create([FromBody] Client client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _clientService.AddClientAsync(client);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Client",
                "Create",
                HttpContext.Request.Path,
                $"Client created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Client>> Update(int id, [FromBody] Client updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _clientService.UpdateClientAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Client",
                "Update",
                HttpContext.Request.Path,
                $"Client {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _clientService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Client",
                "Delete",
                HttpContext.Request.Path,
                $"Client {id} soft-deleted");

            return NoContent();
        }
    }
}

