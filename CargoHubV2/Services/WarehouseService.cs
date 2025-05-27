using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public WarehouseService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Warehouse>> GetWarehousesAsync(int limit)
        {
            return await _context.Warehouses
                .Where(e => !e.IsDeleted)
                .Include(w => w.ContactPersons)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            return await _context.Warehouses
                .Where(e => !e.IsDeleted)
                .Include(w => w.ContactPersons)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .Where(e => !e.IsDeleted && e.Id == id)
                .Include(w => w.ContactPersons)
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
            entity.HazardClassification = updated.HazardClassification;
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

        public async Task<(bool IsCompliant, List<Inventory> ForbiddenItems)> CheckHazardComplianceAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse == null)
            {
                return (false, null);
            }

            var forbiddenItems = await _context.Inventories
                .Where(inv => inv.InventoryLocations.Any(il => il.Location.WarehouseId == warehouseId)
                            && (int)inv.HazardClassification > (int)warehouse.HazardClassification)
                .ToListAsync();

            return (forbiddenItems.Count == 0, forbiddenItems);
        }
    }
}
