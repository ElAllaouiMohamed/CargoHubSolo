namespace CargohubV2.DataConverters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using CargohubV2.Contexts;
    using CargohubV2.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class MultiFormatDateConverter : IsoDateTimeConverter
    {
        public MultiFormatDateConverter()
        {
            DateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime) || objectType == typeof(DateTime?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var dateStr = reader.Value.ToString();
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.AssumeUniversal, out var dt) ||
                DateTime.TryParseExact(dateStr, "yyyy-MM-ddTHH:mm:ssZ", null, DateTimeStyles.AssumeUniversal, out dt))
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }

            throw new JsonSerializationException($"Unable to parse '{dateStr}' as a date.");
        }
    }

    public class DataToJSON
    {
        public static DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }

        public static List<T> LoadDataFromFile<T>(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new MultiFormatDateConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };

            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<T>>(json, settings);
            }
        }

        public static void ImportData(CargoHubDbContext context)
        {
            var clients = LoadDataFromFile<Client>("data/clients.json");
            foreach (var client in clients)
            {
                client.CreatedAt = ToUtc(client.CreatedAt);
                client.UpdatedAt = ToUtc(client.UpdatedAt);
                client.Id = 0;
            }
            context.Clients.AddRange(clients);
            context.SaveChanges();

            var inventories = LoadDataFromFile<Inventory>("data/inventories.json");
            foreach (var inventory in inventories)
            {
                inventory.CreatedAt = ToUtc(inventory.CreatedAt);
                inventory.UpdatedAt = ToUtc(inventory.UpdatedAt);
                inventory.Id = 0;
            }
            context.Inventories.AddRange(inventories);
            context.SaveChanges();

            var suppliers = LoadDataFromFile<Supplier>("data/suppliers.json");
            foreach (var supplier in suppliers)
            {
                supplier.CreatedAt = ToUtc(supplier.CreatedAt);
                supplier.UpdatedAt = ToUtc(supplier.UpdatedAt);
                supplier.Id = 0;
            }
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            var itemGroups = LoadDataFromFile<Item_Group>("data/item_groups.json");
            foreach (var itemGroup in itemGroups)
            {
                itemGroup.CreatedAt = ToUtc(itemGroup.CreatedAt);
                itemGroup.UpdatedAt = ToUtc(itemGroup.UpdatedAt);
                itemGroup.Id = 0;
            }
            context.ItemGroups.AddRange(itemGroups);
            context.SaveChanges();

            var itemLines = LoadDataFromFile<Item_Line>("data/item_lines.json");
            foreach (var itemLine in itemLines)
            {
                itemLine.CreatedAt = ToUtc(itemLine.CreatedAt);
                itemLine.UpdatedAt = ToUtc(itemLine.UpdatedAt);
                itemLine.Id = 0;
            }
            context.ItemLines.AddRange(itemLines);
            context.SaveChanges();

            var itemTypes = LoadDataFromFile<Item_Type>("data/item_types.json");
            foreach (var itemType in itemTypes)
            {
                itemType.CreatedAt = ToUtc(itemType.CreatedAt);
                itemType.UpdatedAt = ToUtc(itemType.UpdatedAt);
                itemType.Id = 0;
            }
            context.ItemTypes.AddRange(itemTypes);
            context.SaveChanges();

            var items = LoadDataFromFile<Item>("data/items.json");
            foreach (var item in items)
            {
                item.CreatedAt = ToUtc(item.CreatedAt);
                item.UpdatedAt = ToUtc(item.UpdatedAt);
                item.Id = 0;
            }
            context.Items.AddRange(items);
            context.SaveChanges();

            var warehouses = LoadDataFromFile<Warehouse>("data/warehouses.json");
            foreach (var warehouse in warehouses)
            {
                warehouse.CreatedAt = ToUtc(warehouse.CreatedAt);
                warehouse.UpdatedAt = ToUtc(warehouse.UpdatedAt);
                warehouse.Id = 0;
            }
            context.Warehouses.AddRange(warehouses);
            context.SaveChanges();

            var orders = LoadDataFromFile<Order>("data/orders.json");
            foreach (var order in orders)
            {
                order.CreatedAt = ToUtc(order.CreatedAt);
                order.UpdatedAt = ToUtc(order.UpdatedAt);
                order.RequestDate = ToUtc(order.RequestDate);
                order.OrderDate = ToUtc(order.OrderDate);
                order.Id = 0;
            }
            context.Orders.AddRange(orders);
            context.SaveChanges();

            var shipments = LoadDataFromFile<Shipment>("data/shipments.json");
            foreach (var shipment in shipments)
            {
                shipment.CreatedAt = ToUtc(shipment.CreatedAt);
                shipment.UpdatedAt = ToUtc(shipment.UpdatedAt);
                shipment.Id = 0;
            }
            context.Shipments.AddRange(shipments);
            context.SaveChanges();

            var transfers = LoadDataFromFile<Transfer>("data/transfers.json");
            foreach (var transfer in transfers)
            {
                transfer.CreatedAt = ToUtc(transfer.CreatedAt);
                transfer.UpdatedAt = ToUtc(transfer.UpdatedAt);
                transfer.Id = 0;
            }
            context.Transfers.AddRange(transfers);
            context.SaveChanges();

            var locations = LoadDataFromFile<Location>("data/locations.json");
            foreach (var location in locations)
            {
                location.CreatedAt = ToUtc(location.CreatedAt);
                location.UpdatedAt = ToUtc(location.UpdatedAt);
                location.Id = 0;
            }
            context.Locations.AddRange(locations);
            context.SaveChanges();


            var shipmentStocks = new List<ShipmentStock>();
            foreach (var shipment in shipments)
            {
                if (shipment.Stocks != null)
                {
                    foreach (var item in shipment.Stocks)
                    {
                        shipmentStocks.Add(new ShipmentStock
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            ShipmentId = shipment.Id,
                        });
                    }
                }
            }
            context.Stocks.AddRange(shipmentStocks);
            context.SaveChanges();

            var transferStocks = new List<TransferStock>();
            foreach (var transfer in transfers)
            {
                if (transfer.Stocks != null)
                {
                    foreach (var item in transfer.Stocks)
                    {
                        transferStocks.Add(new TransferStock
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            TransferId = transfer.Id,
                        });
                    }
                }
            }
            context.Stocks.AddRange(transferStocks);
            context.SaveChanges();

            var orderStocks = new List<OrderStock>();
            foreach (var order in orders)
            {
                if (order.Stocks != null)
                {
                    foreach (var item in order.Stocks)
                    {
                        orderStocks.Add(new OrderStock
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            OrderId = order.Id,
                        });
                    }
                }
            }
            context.Stocks.AddRange(orderStocks);
            context.SaveChanges();
        }
    }

    public class DataValidator
    {
        public static void ValidateDataTypes(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("total_package_weight") || !(data["total_package_weight"] is double))
            {
                throw new ArgumentException("total_package_weight moet een decimaal zijn.");
            }

            if (!data.ContainsKey("carrier_code") || !(data["carrier_code"] is string))
            {
                throw new ArgumentException("carrier_code moet een string zijn.");
            }
        }

        public static void ValidateRequiredFields(Dictionary<string, object> data)
        {
            var requiredFields = new List<string> { "order_id", "shipment_status" };

            foreach (var field in requiredFields)
            {
                if (!data.ContainsKey(field))
                {
                    throw new ArgumentException($"{field} is a required field.");
                }
            }
        }

        public static void ValidateSupplierData(Dictionary<string, object> data)
        {
            var requiredFields = new List<string> { "code", "name", "address", "city", "country" };

            foreach (var field in requiredFields)
            {
                if (!data.ContainsKey(field))
                {
                    throw new ArgumentException($"{field} is a required field.");
                }
            }
        }

        public static void ValidateUpdateData(Dictionary<string, object> existingData, Dictionary<string, object> newData)
        {
            foreach (var key in newData.Keys)
            {
                if (!existingData.ContainsKey(key))
                {
                    throw new KeyNotFoundException($"{key} is not a valid key to update.");
                }
            }
        }
    }
}

