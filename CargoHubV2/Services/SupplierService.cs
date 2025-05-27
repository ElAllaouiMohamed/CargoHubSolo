using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public SupplierService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Supplier>> GetSuppliersAsync(int limit)
        {
            return await _context.Suppliers
                .Where(e => !e.IsDeleted)
                .Include(s => s.ContactPersons) // contactpersonen worde mee geladen
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers
                .Where(e => !e.IsDeleted)
                .Include(s => s.ContactPersons)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers
                .Where(e => !e.IsDeleted && e.Id == id)
                .Include(s => s.ContactPersons)
                .FirstOrDefaultAsync();
        }

        public async Task<Supplier> AddSupplierAsync(Supplier supplier)
        {
            supplier.CreatedAt = DateTime.UtcNow;
            supplier.UpdatedAt = DateTime.UtcNow;
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            await _loggingService.LogAsync("system", "Supplier", "Create", "/api/v1/suppliers", $"Created supplier {supplier.Id}");
            return supplier;
        }

        public async Task<Supplier?> UpdateSupplierAsync(int id, Supplier updated)
        {
            var entity = await _context.Suppliers.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Code = updated.Code;
            entity.Name = updated.Name;
            entity.Address = updated.Address;
            entity.AddressExtra = updated.AddressExtra;
            entity.City = updated.City;
            entity.ZipCode = updated.ZipCode;
            entity.Province = updated.Province;
            entity.Country = updated.Country;
            entity.ContactName = updated.ContactName;
            entity.PhoneNumber = updated.PhoneNumber;
            entity.Reference = updated.Reference;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Supplier", "Update", $"/api/v1/suppliers/{id}", $"Updated supplier {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Suppliers.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Supplier", "Delete", $"/api/v1/suppliers/{id}", $"Soft deleted supplier {id}");
            return true;
        }
    }
}

