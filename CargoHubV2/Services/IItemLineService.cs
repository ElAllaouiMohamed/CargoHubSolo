using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IItemLineService
    {
        Task<List<Item_Line>> GetItemLinesAsync(int limit);
        Task<List<Item_Line>> GetAllItemLinesAsync();
        Task<Item_Line> GetByIdAsync(int id);
        Task<Item_Line> AddItemLineAsync(Item_Line itemLine);
        Task<Item_Line> UpdateItemLineAsync(int id, Item_Line updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
