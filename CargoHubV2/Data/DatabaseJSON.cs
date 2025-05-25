using CargoHubV2.Data;
using CargoHubV2.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CargoHubV2.Seeding
{
    public class DatabaseJSON
    {
        public static async Task SeedAsync(CargoHubContext context)
        {
            if (!context.Clients.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/clients.json");
                var data = JsonConvert.DeserializeObject<List<Client>>(json);
                context.Clients.AddRange(data);
            }

            if (!context.Suppliers.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/suppliers.json");
                var data = JsonConvert.DeserializeObject<List<Supplier>>(json);
                context.Suppliers.AddRange(data);
            }

            if (!context.Items.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/items.json");
                var data = JsonConvert.DeserializeObject<List<Item>>(json);
                context.Items.AddRange(data);
            }

            if (!context.Orders.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/orders.json");
                var data = JsonConvert.DeserializeObject<List<Order>>(json);
                context.Orders.AddRange(data);
            }

            if (!context.Inventories.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/inventories.json");
                var data = JsonConvert.DeserializeObject<List<Inventory>>(json);
                context.Inventories.AddRange(data);
            }

            if (!context.Shipments.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/shipments.json");
                var data = JsonConvert.DeserializeObject<List<Shipment>>(json);
                context.Shipments.AddRange(data);
            }

            if (!context.Warehouses.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/warehouses.json");
                var data = JsonConvert.DeserializeObject<List<Warehouse>>(json);
                context.Warehouses.AddRange(data);
            }

            if (!context.Locations.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/locations.json");
                var data = JsonConvert.DeserializeObject<List<Location>>(json);
                context.Locations.AddRange(data);
            }

            if (!context.ItemTypes.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/item_types.json");
                var data = JsonConvert.DeserializeObject<List<ItemType>>(json);
                context.ItemTypes.AddRange(data);
            }

            if (!context.ItemGroups.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/item_groups.json");
                var data = JsonConvert.DeserializeObject<List<ItemGroup>>(json);
                context.ItemGroups.AddRange(data);
            }
            
            if (!context.Transfers.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/transfers.json");
                var data = JsonConvert.DeserializeObject<List<Transfer>>(json);
                context.Transfers.AddRange(data);
            }

            if (!context.ItemLines.Any())
            {
                var json = await File.ReadAllTextAsync("jsonfiles/item_lines.json");
                var data = JsonConvert.DeserializeObject<List<ItemLine>>(json);
                context.ItemLines.AddRange(data);
            }

            await context.SaveChangesAsync();
        }
    }
}
