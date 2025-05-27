using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class ShipmentService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public ShipmentService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Shipment>> GetShipmentsAsync(int limit)
        {
            return await _context.Shipments
                .Where(e => !e.IsDeleted)
                .Include(e => e.Stocks)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Shipment>> GetAllShipmentsAsync()
        {
            return await _context.Shipments
                .Where(e => !e.IsDeleted)
                .Include(e => e.Stocks)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(int id)
        {
            return await _context.Shipments
                .Where(e => !e.IsDeleted && e.Id == id)
                .Include(e => e.Stocks)
                .FirstOrDefaultAsync();
        }

        public async Task<Shipment> AddShipmentAsync(Shipment shipment)
        {
            shipment.CreatedAt = DateTime.UtcNow;
            shipment.UpdatedAt = DateTime.UtcNow;
            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Shipment", "Create", "/api/v1/shipments", $"Created shipment {shipment.Id}");
            return shipment;
        }

        public async Task<Shipment?> UpdateShipmentAsync(int id, Shipment updated)
        {
            var entity = await _context.Shipments.Include(e => e.Stocks).FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.OrderId = updated.OrderId;
            entity.SourceId = updated.SourceId;
            entity.OrderDate = updated.OrderDate;
            entity.RequestDate = updated.RequestDate;
            entity.ShipmentDate = updated.ShipmentDate;
            entity.ShipmentType = updated.ShipmentType;
            entity.ShipmentStatus = updated.ShipmentStatus;
            entity.Notes = updated.Notes;
            entity.CarrierCode = updated.CarrierCode;
            entity.CarrierDescription = updated.CarrierDescription;
            entity.ServiceCode = updated.ServiceCode;
            entity.PaymentType = updated.PaymentType;
            entity.TransferMode = updated.TransferMode;
            entity.TotalPackageCount = updated.TotalPackageCount;
            entity.TotalPackageWeight = updated.TotalPackageWeight;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.Stocks = updated.Stocks ?? new List<ShipmentStock>();

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Shipment", "Update", $"/api/v1/shipments/{id}", $"Updated shipment {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Shipments.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Shipment", "Delete", $"/api/v1/shipments/{id}", $"Soft deleted shipment {id}");
            return true;
        }
    }
}

