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
        private readonly LoggingService _loggingService;

        public ClientService(CargoHubDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

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
    }
}
