using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IItemGroupService
    {
        Task<List<Item_Group>> GetItemGroupsAsync(int limit);
        Task<List<Item_Group>> GetAllItemGroupsAsync();
        Task<Item_Group> GetByIdAsync(int id);
        Task<Item_Group> AddItemGroupAsync(Item_Group itemGroup);
        Task<Item_Group> UpdateItemGroupAsync(int id, Item_Group updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
