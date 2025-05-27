using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IShipmentService
    {
        Task<List<Shipment>> GetShipmentsAsync(int limit);
        Task<List<Shipment>> GetAllShipmentsAsync();
        Task<Shipment> GetByIdAsync(int id);
        Task<Shipment> AddShipmentAsync(Shipment shipment);
        Task<Shipment> UpdateShipmentAsync(int id, Shipment updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
