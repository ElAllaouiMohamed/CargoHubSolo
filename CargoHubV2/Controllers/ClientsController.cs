using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerOperation(Summary = "Get all clients", Description = "Returns a list of clients with optional limit.")]
        [SwaggerResponse(200, "List of clients", typeof(IEnumerable<Client>))]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _clientService.GetClientsAsync(limit.Value)
                : await _clientService.GetAllClientsAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get client by ID", Description = "Returns a single client by their unique identifier.")]
        [SwaggerResponse(200, "Client found", typeof(Client))]
        [SwaggerResponse(404, "Client not found")]
        public async Task<ActionResult<Client>> GetById(int id)
        {
            var entity = await _clientService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // Contactpersonen ophalen voor clie
        [HttpGet("{clientId:int}/contactpersons")]
        [SwaggerOperation(Summary = "Get contact persons by client ID", Description = "Returns a list of contact persons for a specific client.")]
        [SwaggerResponse(200, "List of contact persons", typeof(IEnumerable<ContactPerson>))]
        [SwaggerResponse(404, "Client not found")]
        public async Task<ActionResult<IEnumerable<ContactPerson>>> GetContactPersons(int clientId)
        {
            var contactPersons = await _clientService.GetContactPersonsByClientIdAsync(clientId);
            return Ok(contactPersons);
        }

        // Contactpersoon toevoegen aan client
        [HttpPost("{clientId:int}/contactpersons")]
        [SwaggerOperation(Summary = "Add contact person to client", Description = "Adds a new contact person to a specific client.")]
        [SwaggerResponse(201, "Contact person added", typeof(ContactPerson))]
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
        [SwaggerOperation(Summary = "Update contact person", Description = "Updates an existing contact person by their unique identifier.")]
        [SwaggerResponse(200, "Contact person updated", typeof(ContactPerson))]
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
        [SwaggerOperation(Summary = "Delete contact person", Description = "Deletes a contact person by their unique identifier.")]
        [SwaggerResponse(204, "Contact person deleted")]
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
        [SwaggerOperation(Summary = "Create a new client", Description = "Creates a new client and returns the created client object.")]
        [SwaggerResponse(201, "Client created", typeof(Client))]
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
        [SwaggerOperation(Summary = "Update an existing client", Description = "Updates an existing client by their unique identifier.")]
        [SwaggerResponse(200, "Client updated", typeof(Client))]
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
        [SwaggerOperation(Summary = "Soft delete a client", Description = "Soft deletes a client by their unique identifier. The client is not permanently removed but marked as deleted.")]
        [SwaggerResponse(204, "Client soft-deleted")]
        [SwaggerResponse(404, "Client not found")]
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

