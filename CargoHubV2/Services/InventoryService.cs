using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class InventoryService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public InventoryService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Inventory>> GetInventoriesAsync(int limit)
        {
            return await _context.Inventories
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetAllInventoriesAsync()
        {
            return await _context.Inventories
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByIdAsync(int id)
        {
            return await _context.Inventories
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Inventory> AddInventoryAsync(Inventory inventory)
        {
            inventory.CreatedAt = DateTime.UtcNow;
            inventory.UpdatedAt = DateTime.UtcNow;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Inventory", "Create", "/api/v1/inventories", $"Created inventory {inventory.Id}");
            return inventory;
        }

        public async Task<Inventory?> UpdateInventoryAsync(int id, Inventory updated)
        {
            var entity = await _context.Inventories.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.ItemId = updated.ItemId;
            entity.Description = updated.Description;
            entity.ItemReference = updated.ItemReference;
            entity.Locations = updated.Locations;
            entity.TotalOnHand = updated.TotalOnHand;
            entity.TotalExpected = updated.TotalExpected;
            entity.TotalOrdered = updated.TotalOrdered;
            entity.TotalAllocated = updated.TotalAllocated;
            entity.TotalAvailable = updated.TotalAvailable;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Inventory", "Update", $"/api/v1/inventories/{id}", $"Updated inventory {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Inventories.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Inventory", "Delete", $"/api/v1/inventories/{id}", $"Soft deleted inventory {id}");
            return true;
        }


        public async Task<List<InventoryLocation>> GetInventoryLocationsAsync(int inventoryId)
        {
            return await _context.InventoryLocations
                .Include(il => il.Location)
                .Where(il => il.InventoryId == inventoryId)
                .ToListAsync();
        }

        public async Task<InventoryLocation> AddInventoryLocationAsync(InventoryLocation inventoryLocation)
        {
            inventoryLocation.CreatedAt = DateTime.UtcNow;
            inventoryLocation.UpdatedAt = DateTime.UtcNow;
            _context.InventoryLocations.Add(inventoryLocation);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "InventoryLocation", "Create", $"/api/v1/inventories/{inventoryLocation.InventoryId}/locations", $"Added inventory location {inventoryLocation.Id}");
            return inventoryLocation;
        }

        public async Task<bool> SoftDeleteInventoryLocationAsync(int id)
        {
            var entity = await _context.InventoryLocations.FirstOrDefaultAsync(il => il.Id == id && !il.IsDeleted);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "InventoryLocation", "Delete", $"/api/v1/inventorylocations/{id}", $"Soft deleted inventory location {id}");
            return true;
        }
    }
}

