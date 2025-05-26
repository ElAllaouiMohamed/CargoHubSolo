using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class ItemTypeService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public ItemTypeService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Item_Type>> GetItemTypesAsync(int limit)
        {
            return await _context.ItemTypes
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Item_Type>> GetAllItemTypesAsync()
        {
            return await _context.ItemTypes
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Item_Type?> GetByIdAsync(int id)
        {
            return await _context.ItemTypes
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Item_Type> AddItemTypeAsync(Item_Type itemType)
        {
            itemType.CreatedAt = DateTime.UtcNow;
            itemType.UpdatedAt = DateTime.UtcNow;
            _context.ItemTypes.Add(itemType);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Type", "Create", "/api/v1/itemtypes", $"Created item_type {itemType.Id}");
            return itemType;
        }

        public async Task<Item_Type?> UpdateItemTypeAsync(int id, Item_Type updated)
        {
            var entity = await _context.ItemTypes.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Name = updated.Name;
            entity.Description = updated.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Type", "Update", $"/api/v1/itemtypes/{id}", $"Updated item_type {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.ItemTypes.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Type", "Delete", $"/api/v1/itemtypes/{id}", $"Soft deleted item_type {id}");
            return true;
        }
    }
}
