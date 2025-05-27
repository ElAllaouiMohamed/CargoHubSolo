using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class LocationService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public LocationService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Location>> GetLocationsAsync(int limit)
        {
            return await _context.Locations
                .Where(e => !e.IsDeleted)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Locations
                .Where(e => !e.IsDeleted && e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Location> AddLocationAsync(Location location)
        {
            location.CreatedAt = DateTime.UtcNow;
            location.UpdatedAt = DateTime.UtcNow;
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Location", "Create", "/api/v1/locations", $"Created location {location.Id}");
            return location;
        }

        public async Task<Location?> UpdateLocationAsync(int id, Location updated)
        {
            var entity = await _context.Locations.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.WarehouseId = updated.WarehouseId;
            entity.Code = updated.Code;
            entity.Name = updated.Name;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Location", "Update", $"/api/v1/locations/{id}", $"Updated location {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Locations.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Location", "Delete", $"/api/v1/locations/{id}", $"Soft deleted location {id}");
            return true;
        }
    }
}

