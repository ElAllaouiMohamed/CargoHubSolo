using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface ILocationService
    {
        Task<List<Location>> GetLocationsAsync(int limit);
        Task<List<Location>> GetAllLocationsAsync();
        Task<Location?> GetByIdAsync(int id);
        Task<Location> AddLocationAsync(Location location);
        Task<Location?> UpdateLocationAsync(int id, Location updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }

}
