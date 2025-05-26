using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CargohubV2.Services
{
    public class TransferService
    {
        private readonly CargoHubDbContext _context;
        private readonly ILoggingService _loggingService;

        public TransferService(CargoHubDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<List<Transfer>> GetTransfersAsync(int limit)
        {
            return await _context.Transfers
                .Where(e => !e.IsDeleted)
                .Include(e => e.Stocks)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Transfer>> GetAllTransfersAsync()
        {
            return await _context.Transfers
                .Where(e => !e.IsDeleted)
                .Include(e => e.Stocks)
                .ToListAsync();
        }

        public async Task<Transfer?> GetByIdAsync(int id)
        {
            return await _context.Transfers
                .Where(e => !e.IsDeleted && e.Id == id)
                .Include(e => e.Stocks)
                .FirstOrDefaultAsync();
        }

        public async Task<Transfer> AddTransferAsync(Transfer transfer)
        {
            transfer.CreatedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;
            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Transfer", "Create", "/api/v1/transfers", $"Created transfer {transfer.Id}");
            return transfer;
        }

        public async Task<Transfer?> UpdateTransferAsync(int id, Transfer updated)
        {
            var entity = await _context.Transfers.Include(e => e.Stocks).FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return null;

            entity.Reference = updated.Reference;
            entity.TransferFrom = updated.TransferFrom;
            entity.TransferTo = updated.TransferTo;
            entity.TransferStatus = updated.TransferStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Stocks = updated.Stocks ?? new List<TransferStock>();

            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Transfer", "Update", $"/api/v1/transfers/{id}", $"Updated transfer {id}");
            return entity;
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await _context.Transfers.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync("system", "Transfer", "Delete", $"/api/v1/transfers/{id}", $"Soft deleted transfer {id}");
            return true;
        }
    }
}
