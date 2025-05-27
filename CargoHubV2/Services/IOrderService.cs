using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync(int limit);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetByIdAsync(int id);
        Task<Order> AddOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(int id, Order updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
