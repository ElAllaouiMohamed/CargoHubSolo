using Microsoft.EntityFrameworkCore;
using CargoHubV2.Models;

namespace CargoHubV2.Data
{
    public class CargoHubContext : DbContext
    {
        public CargoHubContext(DbContextOptions<CargoHubContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<ItemLine> ItemLines { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TransferItem> TransferItems { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseContact> WarehouseContacts { get; set; }



    }
}
