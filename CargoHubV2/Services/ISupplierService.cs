using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface ISupplierService
    {
        Task<List<Supplier>> GetSuppliersAsync(int limit);
        Task<List<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetByIdAsync(int id);
        Task<Supplier> AddSupplierAsync(Supplier supplier);
        Task<Supplier?> UpdateSupplierAsync(int id, Supplier updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }

}
