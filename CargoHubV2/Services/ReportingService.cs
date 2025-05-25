using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargohubV2.Models;
using CargohubV2.Contexts;

namespace CargohubV2.Services
{
    public class ReportingService
    {
        private readonly CargoHubDbContext _context;

        public ReportingService(CargoHubDbContext context)
        {
            _context = context;
        }

        public ReportResult GetWarehouseReport(int warehouseId)
        {
            var orders = _context.Orders
                .Where(o => o.WarehouseId == warehouseId)
                .ToList();

            int totalOrders = orders.Count;
            int totalItems = orders
                .Where(o => o.Stocks != null)
                .Sum(o => o.Stocks.Sum(s => s.Quantity));

            return new ReportResult
            {
                TotalOrders = totalOrders,
                TotalItems = totalItems
            };
        }

        public string GenerateCsvReport(int warehouseId)
        {
            var orders = _context.Orders
                .Where(o => o.WarehouseId == warehouseId)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Order ID, Warehouse ID, Order Price");
            foreach (var order in orders)
            {
                sb.AppendLine($"{order.Id}, {order.WarehouseId}, {order.TotalAmount}");
            }
            return sb.ToString();
        }

        public RevenueResult GetRevenueBetweenDates(DateTime start, DateTime end)
        {
            var total = _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
                .Sum(o => o.TotalAmount);

            return new RevenueResult
            {
                TotalRevenue = Math.Round(total, 2)
            };
        }
    }

    public class ReportResult
    {
        public int TotalOrders { get; set; }
        public int TotalItems { get; set; }
    }

    public class RevenueResult
    {
        public double TotalRevenue { get; set; }
    }
}
