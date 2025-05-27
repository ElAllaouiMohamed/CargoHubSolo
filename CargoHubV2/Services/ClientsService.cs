using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class ClientService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public ClientService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // Client CRUD

        public async Task<List<Client>> GetClientsAsync(int limit)
        {
            return await _context.Clients
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _context.Clients
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Client> AddClientAsync(Client client)
        {
            client.CreatedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Client", "Create", "/api/v1/clients", $"Created client {client.Id}");
            return client;
        }

        public async Task<Client?> UpdateClientAsync(int id, Client updated)
        {
            var entity = await _context.Clients.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Name = updated.Name;
            entity.Address = updated.Address;
            entity.City = updated.City;
            entity.ZipCode = updated.ZipCode;
            entity.Province = updated.Province;
            entity.Country = updated.Country;
            entity.ContactName = updated.ContactName;
            entity.ContactPhone = updated.ContactPhone;
            entity.ContactEmail = updated.ContactEmail;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Client", "Update", $"/api/v1/clients/{id}", $"Updated client {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Clients.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Client", "Delete", $"/api/v1/clients/{id}", $"Soft deleted client {id}");
            return true;
        }


        // ContactPerson CRUD voor Client

        public async Task<List<ContactPerson>> GetContactPersonsByClientIdAsync(int clientId)
        {
            return await _context.ContactPersons
                .Where(cp => cp.ClientId == clientId && !cp.IsDeleted)
                .ToListAsync();
        }

        public async Task<ContactPerson> AddContactPersonToClientAsync(int clientId, ContactPerson contactPerson)
        {
            contactPerson.ClientId = clientId;
            contactPerson.IsDeleted = false;
            _context.ContactPersons.Add(contactPerson);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "ContactPerson", "Create", $"/api/v1/clients/{clientId}/contactpersons", $"Added contact person {contactPerson.Name} to client {clientId}");
            return contactPerson;
        }

        public async Task<ContactPerson?> UpdateContactPersonAsync(int contactPersonId, ContactPerson updated)
        {
            var existing = await _context.ContactPersons.FindAsync(contactPersonId);
            if (existing == null || existing.IsDeleted) return null;

            existing.Name = updated.Name;
            existing.Function = updated.Function;
            existing.Email = updated.Email;
            existing.Phone = updated.Phone;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "ContactPerson", "Update", $"/api/v1/contactpersons/{contactPersonId}", $"Updated contact person {contactPersonId}");
            return existing;
        }

        public async Task<bool> DeleteContactPersonAsync(int contactPersonId)
        {
            var existing = await _context.ContactPersons.FindAsync(contactPersonId);
            if (existing == null || existing.IsDeleted) return false;

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "ContactPerson", "Delete", $"/api/v1/contactpersons/{contactPersonId}", $"Deleted contact person {contactPersonId}");
            return true;
        }
    }
}

