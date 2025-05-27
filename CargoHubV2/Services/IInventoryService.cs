using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetInventoriesAsync(int limit);
        Task<List<Inventory>> GetAllInventoriesAsync();
        Task<Inventory> GetByIdAsync(int id);
        Task<Inventory> AddInventoryAsync(Inventory inventory);
        Task<Inventory> UpdateInventoryAsync(int id, Inventory updated);
        Task<bool> SoftDeleteByIdAsync(int id);
        Task<List<InventoryLocation>> GetInventoryLocationsAsync(int inventoryId);
    }
}
