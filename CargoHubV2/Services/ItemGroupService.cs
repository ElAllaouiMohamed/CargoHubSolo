using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class ItemGroupService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public ItemGroupService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Item_Group>> GetItemGroupsAsync(int limit)
        {
            return await _context.ItemGroups
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Item_Group>> GetAllItemGroupsAsync()
        {
            return await _context.ItemGroups
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Item_Group?> GetByIdAsync(int id)
        {
            return await _context.ItemGroups
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Item_Group> AddItemGroupAsync(Item_Group itemGroup)
        {
            itemGroup.CreatedAt = DateTime.UtcNow;
            itemGroup.UpdatedAt = DateTime.UtcNow;
            _context.ItemGroups.Add(itemGroup);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Group", "Create", "/api/v1/itemgroups", $"Created item_group {itemGroup.Id}");
            return itemGroup;
        }

        public async Task<Item_Group?> UpdateItemGroupAsync(int id, Item_Group updated)
        {
            var entity = await _context.ItemGroups.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Name = updated.Name;
            entity.Description = updated.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Group", "Update", $"/api/v1/itemgroups/{id}", $"Updated item_group {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.ItemGroups.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Group", "Delete", $"/api/v1/itemgroups/{id}", $"Soft deleted item_group {id}");
            return true;
        }
    }
}

