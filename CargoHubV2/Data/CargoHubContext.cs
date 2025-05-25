using CargoHubV2.Models;
using Microsoft.EntityFrameworkCore;

namespace CargoHubV2.Data
{
    public class CargoHubContext : DbContext
    {
        public CargoHubContext(DbContextOptions<CargoHubContext> options) : base(options) {}

        public DbSet<Client> Clients { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
    }
}
