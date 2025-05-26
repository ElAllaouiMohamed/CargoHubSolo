using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class ItemLineService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public ItemLineService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Item_Line>> GetItemLinesAsync(int limit)
        {
            return await _context.ItemLines
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Item_Line>> GetAllItemLinesAsync()
        {
            return await _context.ItemLines
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Item_Line?> GetByIdAsync(int id)
        {
            return await _context.ItemLines
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Item_Line> AddItemLineAsync(Item_Line itemLine)
        {
            itemLine.CreatedAt = DateTime.UtcNow;
            itemLine.UpdatedAt = DateTime.UtcNow;
            _context.ItemLines.Add(itemLine);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Line", "Create", "/api/v1/itemlines", $"Created item_line {itemLine.Id}");
            return itemLine;
        }

        public async Task<Item_Line?> UpdateItemLineAsync(int id, Item_Line updated)
        {
            var entity = await _context.ItemLines.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Name = updated.Name;
            entity.Description = updated.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Line", "Update", $"/api/v1/itemlines/{id}", $"Updated item_line {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.ItemLines.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item_Line", "Delete", $"/api/v1/itemlines/{id}", $"Soft deleted item_line {id}");
            return true;
        }
    }
}
