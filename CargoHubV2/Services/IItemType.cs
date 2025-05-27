using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IItemTypeService
    {
        Task<List<Item_Type>> GetItemTypesAsync(int limit);
        Task<List<Item_Type>> GetAllItemTypesAsync();
        Task<Item_Type?> GetByIdAsync(int id);
        Task<Item_Type> AddItemTypeAsync(Item_Type itemType);
        Task<Item_Type?> UpdateItemTypeAsync(int id, Item_Type updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
