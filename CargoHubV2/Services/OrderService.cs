using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class OrderService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public OrderService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Order>> GetOrdersAsync(int limit)
        {
            return await _context.Orders
                .Where(e => !e.IsDeleted)
                .Include(e => e.Stocks)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Where(e => !e.IsDeleted)
                .Include(e => e.Stocks)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Where(e => !e.IsDeleted && e.Id == id)
                .Include(e => e.Stocks)
                .FirstOrDefaultAsync();
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Order", "Create", "/api/v1/orders", $"Created order {order.Id}");
            return order;
        }

        public async Task<Order?> UpdateOrderAsync(int id, Order updated)
        {
            var entity = await _context.Orders.Include(e => e.Stocks).FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.SourceId = updated.SourceId;
            entity.OrderDate = updated.OrderDate;
            entity.RequestDate = updated.RequestDate;
            entity.Reference = updated.Reference;
            entity.Reference_extra = updated.Reference_extra;
            entity.Order_status = updated.Order_status;
            entity.Notes = updated.Notes;
            entity.ShippingNotes = updated.ShippingNotes;
            entity.PickingNotes = updated.PickingNotes;
            entity.WarehouseId = updated.WarehouseId;
            entity.ShipTo = updated.ShipTo;
            entity.BillTo = updated.BillTo;
            entity.ShipmentId = updated.ShipmentId;
            entity.TotalAmount = updated.TotalAmount;
            entity.TotalDiscount = updated.TotalDiscount;
            entity.TotalTax = updated.TotalTax;
            entity.TotalSurcharge = updated.TotalSurcharge;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.Stocks = updated.Stocks ?? new List<OrderStock>();

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Order", "Update", $"/api/v1/orders/{id}", $"Updated order {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Orders.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Order", "Delete", $"/api/v1/orders/{id}", $"Soft deleted order {id}");
            return true;
        }
    }
}

