using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class ItemService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public ItemService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Item>> GetItemsAsync(int limit)
        {
            return await _context.Items
                .Where(e => !e.IsDeleted)
                .Include(e => e.ItemLine)
                .Include(e => e.ItemGroup)
                .Include(e => e.ItemType)
                .Include(e => e.Supplier)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            return await _context.Items
                .Where(e => !e.IsDeleted)
                .Include(e => e.ItemLine)
                .Include(e => e.ItemGroup)
                .Include(e => e.ItemType)
                .Include(e => e.Supplier)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items
                .Where(e => !e.IsDeleted && e.Id == id)
                .Include(e => e.ItemLine)
                .Include(e => e.ItemGroup)
                .Include(e => e.ItemType)
                .Include(e => e.Supplier)
                .FirstOrDefaultAsync();
        }

        public async Task<Item> AddItemAsync(Item item)
        {
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item", "Create", "/api/v1/items", $"Created item {item.Id}");
            return item;
        }

        public async Task<Item?> UpdateItemAsync(int id, Item updated)
        {
            var entity = await _context.Items.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.UId = updated.UId;
            entity.Code = updated.Code;
            entity.Description = updated.Description;
            entity.ShortDescription = updated.ShortDescription;
            entity.UpcCode = updated.UpcCode;
            entity.ModelNumber = updated.ModelNumber;
            entity.CommodityCode = updated.CommodityCode;
            entity.ItemLineId = updated.ItemLineId;
            entity.ItemGroupId = updated.ItemGroupId;
            entity.ItemTypeId = updated.ItemTypeId;
            entity.UnitPurchaseQuantity = updated.UnitPurchaseQuantity;
            entity.UnitOrderQuantity = updated.UnitOrderQuantity;
            entity.PackOrderQuantity = updated.PackOrderQuantity;
            entity.SupplierId = updated.SupplierId;
            entity.SupplierCode = updated.SupplierCode;
            entity.SupplierPartNumber = updated.SupplierPartNumber;
            entity.WeightInKg = updated.WeightInKg;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item", "Update", $"/api/v1/items/{id}", $"Updated item {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Items.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Item", "Delete", $"/api/v1/items/{id}", $"Soft deleted item {id}");
            return true;
        }
    }
}
