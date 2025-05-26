using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_Order
    {
        private CargoHubDbContext _dbContext;
        private OrderService _orderService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestOrderDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            SeedDatabase(_dbContext);
            _orderService = new OrderService(_dbContext);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.ItemsGroups.AddRange(
                new Item_Group { Id = 1, Name = "dummy", Description = "Dummy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Item_Group { Id = 2, Name = "dummy2", Description = "Dummy2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            context.ItemsTypes.AddRange(
                new Item_Type { Id = 1, Name = "dummy", Description = "Dummy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Item_Type { Id = 2, Name = "dummy2", Description = "Dummy2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            context.ItemsLines.AddRange(
                new Item_Line { Id = 1, Name = "dummy", Description = "Dummy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Item_Line { Id = 2, Name = "dummy2", Description = "Dummy2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            context.Items.AddRange(
                new Item {
                    UId = "P000001",
                    Code = "Dummy",
                    Description = "dummy",
                    ShortDescription = "dummy",
                    UpcCode = "null",
                    ModelNumber = "null",
                    CommodityCode = "null",
                    ItemLineId = 1,
                    ItemGroupId = 1,
                    ItemTypeId = 1,
                    UnitPurchaseQuantity = 1,
                    UnitOrderQuantity = 1,
                    PackOrderQuantity = 1,
                    Price = 4.55,
                    WeightInKg = 6.42,
                    SupplierId = 1,
                    SupplierCode = "null",
                    SupplierPartNumber = "null",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item {
                    UId = "P000002",
                    Code = "Dummy2",
                    Description = "dummy2",
                    ShortDescription = "dummy2",
                    UpcCode = "null",
                    ModelNumber = "null",
                    CommodityCode = "null",
                    ItemLineId = 2,
                    ItemGroupId = 2,
                    ItemTypeId = 2,
                    UnitPurchaseQuantity = 1,
                    UnitOrderQuantity = 1,
                    PackOrderQuantity = 1,
                    Price = 5.25,
                    WeightInKg = 3.42,
                    SupplierId = 2,
                    SupplierCode = "null",
                    SupplierPartNumber = "null",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Clients.AddRange(
                new Client {
                    ClientId = 1,
                    Name = "Bianca Nene",
                    Address = "Straat 1",
                    City = "Rotterdam",
                    ZipCode = "3011AB",
                    Province = "Zuid-Holland",
                    Country = "Nederland",
                    ContactName = "Bianca Nene",
                    ContactPhone = "0612345678",
                    ContactEmail = "bianca@example.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Client {
                    ClientId = 2,
                    Name = "Jan Jansen",
                    Address = "Straat 2",
                    City = "Amsterdam",
                    ZipCode = "1012CD",
                    Province = "Noord-Holland",
                    Country = "Nederland",
                    ContactName = "Jan Jansen",
                    ContactPhone = "0698765432",
                    ContactEmail = "jan@example.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Orders.Add(new Order
            {
                Id = 1,
                SourceId = 1,
                OrderDate = DateTime.UtcNow.AddDays(-7),
                RequestDate = DateTime.UtcNow.AddDays(-3),
                Reference = "ORD-12345",
                ReferenceExtra = "Special handling required",
                OrderStatus = "Processing",
                Notes = "Customer prefers expedited shipping.",
                ShippingNotes = "Fragile items. Handle with care.",
                PickingNotes = "Verify quantities before packing.",
                WarehouseId = 1,
                ShipTo = 2001,
                BillTo = 2002,
                ShipmentId = 3001,
                TotalAmount = 500.75,
                TotalDiscount = 50.00,
                TotalTax = 25.50,
                TotalSurcharge = 10.00,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.OrderStocks.AddRange(
                new OrderStock {
                    Id = 1,
                    OrderId = 1,
                    ItemId = "P000001",
                    Quantity = 10
                },
                new OrderStock {
                    Id = 2,
                    OrderId = 1,
                    ItemId = "P000002",
                    Quantity = 15
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task TestGetAllOrders()
        {
            var orders = await _orderService.GetOrdersAsync();
            Assert.AreEqual(1, orders.Count());
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(99999, false)]
        public async Task TestGetOrderById(int orderId, bool exists)
        {
            var order = await _orderService.GetOrderAsync(orderId);
            Assert.AreEqual(exists, order != null);
        }

        [TestMethod]
        public async Task TestGetLocationsForOrder()
        {
            var locations = await _orderService.GetLocationsForOrderItemsAsync(1);
            Assert.AreEqual(2, locations.Count);

            Assert.IsTrue(locations.ContainsKey("P000001"));
            Assert.IsTrue(locations.ContainsKey("P000002"));
        }

        [TestMethod]
        [DataRow(0, true)]
        [DataRow(1, true)]
        [DataRow(2, false)]
        public async Task TestAddOrder(int orderItemListId, bool expectedResult)
        {
            var orderItemLists = new List<List<OrderStock>> {
                new List<OrderStock>(), // empty list
                new List<OrderStock> {
                    new OrderStock { ItemId = "P000001", Quantity = 5 },
                    new OrderStock { ItemId = "P000002", Quantity = 3 }
                },
                new List<OrderStock> {
                    new OrderStock { ItemId = "P000001", Quantity = 5 },
                    new OrderStock { ItemId = "InvalidItem", Quantity = 3 }
                }
            };

            try
            {
                var newOrder = await _orderService.AddOrderAsync(
                    sourceId: 2,
                    orderDate: DateTime.UtcNow.AddDays(-7),
                    requestDate: DateTime.UtcNow.AddDays(-3),
                    reference: "ORD-12345 Test",
                    referenceExtra: "Special handling required Test",
                    orderStatus: "Processing",
                    notes: "Test notes.",
                    shippingNotes: "Test shipping notes.",
                    pickingNotes: "Test picking notes.",
                    warehouseId: 1,
                    shipTo: 2001,
                    billTo: 2002,
                    shipmentId: 3001,
                    totalAmount: 1000.00,
                    totalDiscount: 50.00,
                    totalTax: 25.50,
                    totalSurcharge: 10.00,
                    orderStocks: orderItemLists[orderItemListId]
                );

                Assert.AreEqual(expectedResult, newOrder != null);

                int expectedCount = orderItemLists[orderItemListId].Count;
                int actualCount = await _dbContext.OrderStocks.CountAsync();
                Assert.AreEqual(expectedResult ? expectedCount + 2 : 2, actualCount); // +2 for seeded items

            }
            catch
            {
                Assert.IsFalse(expectedResult);
            }
        }

        [TestMethod]
        [DataRow(1, "Fragile items. Handle with care. Updated", true)]
        [DataRow(999, "Nonexistent", false)]
        public async Task TestUpdateOrder(int orderId, string updatedShippingNotes, bool expectedResult)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(
                    id: orderId,
                    sourceId: 2,
                    orderDate: DateTime.UtcNow.AddDays(-7),
                    requestDate: DateTime.UtcNow.AddDays(-3),
                    reference: "ORD-12345 Updated",
                    referenceExtra: "Special handling required Updated",
                    orderStatus: "InProgress",
                    notes: "Updated notes.",
                    shippingNotes: updatedShippingNotes,
                    pickingNotes: "Updated picking notes.",
                    warehouseId: 1,
                    shipTo: 2001,
                    billTo: 2002,
                    shipmentId: 3001,
                    totalAmount: 1000.00,
                    totalDiscount: 50.00,
                    totalTax = 25.50,
                    totalSurcharge: 10.00
                );

                Assert.AreEqual(expectedResult, updatedOrder != null);
                if (expectedResult)
                {
                    Assert.AreEqual(updatedShippingNotes, updatedOrder.ShippingNotes);
                }
            }
            catch
            {
                Assert.IsFalse(expectedResult);
            }
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(999, false)]
        public async Task TestDeleteOrder(int orderId, bool expectedResult)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);
            Assert.AreEqual(expectedResult, result);
            if (result)
            {
                var order = await _dbContext.Orders.FindAsync(orderId);
                Assert.IsNull(order);
            }
        }
    }
}
