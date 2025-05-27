using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface ITransferService
    {
        Task<List<Transfer>> GetTransfersAsync(int limit);
        Task<List<Transfer>> GetAllTransfersAsync();
        Task<Transfer> GetByIdAsync(int id);
        Task<Transfer> AddTransferAsync(Transfer transfer);
        Task<Transfer> UpdateTransferAsync(int id, Transfer updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
