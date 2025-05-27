using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IItemService
    {
        Task<List<Item>> GetItemsAsync(int limit);
        Task<List<Item>> GetAllItemsAsync();
        Task<Item> GetByIdAsync(int id);
        Task<Item> AddItemAsync(Item item);
        Task<Item> UpdateItemAsync(int id, Item updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
