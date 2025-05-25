using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class WarehouseService
    {
        private readonly CargoHubDbContext _context;
        private readonly LoggingService _loggingService;

        public WarehouseService(CargoHubDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Warehouse>> GetWarehousesAsync(int limit)
        {
            return await _context.Warehouses
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            return await _context.Warehouses
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Warehouse> AddWarehouseAsync(Warehouse warehouse)
        {
            warehouse.CreatedAt = DateTime.UtcNow;
            warehouse.UpdatedAt = DateTime.UtcNow;
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Warehouse", "Create", "/api/v1/warehouses", $"Created warehouse {warehouse.Id}");
            return warehouse;
        }

        public async Task<Warehouse?> UpdateWarehouseAsync(int id, Warehouse updated)
        {
            var entity = await _context.Warehouses.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Code = updated.Code;
            entity.Name = updated.Name;
            entity.Address = updated.Address;
            entity.Zip = updated.Zip;
            entity.City = updated.City;
            entity.Province = updated.Province;
            entity.Country = updated.Country;
            entity.Contact = updated.Contact;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Warehouse", "Update", $"/api/v1/warehouses/{id}", $"Updated warehouse {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Warehouses.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Warehouse", "Delete", $"/api/v1/warehouses/{id}", $"Soft deleted warehouse {id}");
            return true;
        }
    }
}
