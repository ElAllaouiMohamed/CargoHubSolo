using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetWarehousesAsync(int limit);
        Task<List<Warehouse>> GetAllWarehousesAsync();
        Task<Warehouse> GetByIdAsync(int id);
        Task<Warehouse> AddWarehouseAsync(Warehouse warehouse);
        Task<Warehouse> UpdateWarehouseAsync(int id, Warehouse updated);
        Task<bool> SoftDeleteByIdAsync(int id);
        Task<(bool IsCompliant, List<Inventory> ForbiddenItems)> CheckHazardComplianceAsync(int warehouseId);
    }
}
