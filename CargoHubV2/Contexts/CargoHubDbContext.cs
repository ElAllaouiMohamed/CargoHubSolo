using CargohubV2.Models;
using Microsoft.EntityFrameworkCore;

namespace CargohubV2.Contexts
{
    public class CargoHubDbContext : DbContext
    {
        public CargoHubDbContext(DbContextOptions<CargoHubDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryLocation> InventoryLocations { get; set; }
        public DbSet<ContactPerson> ContactPersons { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Item_Group> ItemGroups { get; set; }
        public DbSet<Item_Line> ItemLines { get; set; }
        public DbSet<Item_Type> ItemTypes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Query filters
            modelBuilder.Entity<Client>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Location>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<Inventory>().HasQueryFilter(i => !i.IsDeleted);
            modelBuilder.Entity<Transfer>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Item>().HasQueryFilter(it => !it.IsDeleted);
            modelBuilder.Entity<Warehouse>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<Supplier>().HasQueryFilter(su => !su.IsDeleted);
            modelBuilder.Entity<Item_Group>().HasQueryFilter(ig => !ig.IsDeleted);
            modelBuilder.Entity<Item_Line>().HasQueryFilter(il => !il.IsDeleted);
            modelBuilder.Entity<Item_Type>().HasQueryFilter(it => !it.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<Shipment>().HasQueryFilter(sh => !sh.IsDeleted);

            // Owned types
            modelBuilder.Entity<Warehouse>().OwnsOne(w => w.Contact);

            // Discriminator for Stock inheritance
            modelBuilder.Entity<Stock>()
                .ToTable("Stocks")
                .HasDiscriminator<string>("StockType")
                .HasValue<OrderStock>("Order")
                .HasValue<ShipmentStock>("Shipment")
                .HasValue<TransferStock>("Transfer");

            // InventoryLocation relationships
            modelBuilder.Entity<InventoryLocation>()
                .HasOne(il => il.Inventory)
                .WithMany(i => i.InventoryLocations)
                .HasForeignKey(il => il.InventoryId);

            modelBuilder.Entity<InventoryLocation>()
                .HasOne(il => il.Location)
                .WithMany()
                .HasForeignKey(il => il.LocationId);

            // ContactPerson relationships
            modelBuilder.Entity<ContactPerson>()
                .HasOne(cp => cp.Warehouse)
                .WithMany(w => w.ContactPersons)
                .HasForeignKey(cp => cp.WarehouseId);

            modelBuilder.Entity<ContactPerson>()
                .HasOne(cp => cp.Client)
                .WithMany(c => c.ContactPersons)
                .HasForeignKey(cp => cp.ClientId);

            modelBuilder.Entity<ContactPerson>()
                .HasOne(cp => cp.Supplier)
                .WithMany(s => s.ContactPersons)
                .HasForeignKey(cp => cp.SupplierId);
        }
    }
}

