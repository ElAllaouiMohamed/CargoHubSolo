using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class OrderServiceTests
    {
        private CargoHubDbContext _dbContext;
        private OrderService _orderService;
        private Mock<LoggingService> _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<LoggingService>();
            _orderService = new OrderService(_dbContext, _mockLoggingService.Object);
            SeedDatabase(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Orders.AddRange(
                new Order
                {
                    Id = 1,
                    SourceId = 1,
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    RequestDate = DateTime.UtcNow,
                    Reference = "ORD001",
                    Reference_extra = "Extra1",
                    Order_status = "Pending",
                    Notes = "Note1",
                    ShippingNotes = "ShipNote1",
                    PickingNotes = "PickNote1",
                    WarehouseId = 1,
                    ShipTo = "Client A",
                    BillTo = "Client A",
                    ShipmentId = 1,
                    TotalAmount = 100.50,
                    TotalDiscount = 10.00,
                    TotalTax = 21.00,
                    TotalSurcharge = 5.00,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    Stocks = new List<OrderStock>
                    {
                        new OrderStock { Id = 1, OrderId = 1, ItemId = "1", Quantity = 5 }
                    }
                },
                new Order
                {
                    Id = 2,
                    SourceId = 2,
                    OrderDate = DateTime.UtcNow.AddDays(-2),
                    RequestDate = DateTime.UtcNow.AddDays(-1),
                    Reference = "ORD002",
                    Reference_extra = "Extra2",
                    Order_status = "Shipped",
                    Notes = "Note2",
                    ShippingNotes = "ShipNote2",
                    PickingNotes = "PickNote2",
                    WarehouseId = 2,
                    ShipTo = "Client B",
                    BillTo = "Client B",
                    ShipmentId = 2,
                    TotalAmount = 200.75,
                    TotalDiscount = 20.00,
                    TotalTax = 42.00,
                    TotalSurcharge = 10.00,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    Stocks = new List<OrderStock>
                    {
                        new OrderStock { Id = 2, OrderId = 2, ItemId = "2", Quantity = 10 }
                    }
                },
                new Order
                {
                    Id = 3,
                    SourceId = 3,
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    RequestDate = DateTime.UtcNow.AddDays(-2),
                    Reference = "ORD003",
                    Reference_extra = "Extra3",
                    Order_status = "Cancelled",
                    Notes = "Note3",
                    ShippingNotes = "ShipNote3",
                    PickingNotes = "PickNote3",
                    WarehouseId = 3,
                    ShipTo = "Client C",
                    BillTo = "Client C",
                    ShipmentId = 3,
                    TotalAmount = 150.25,
                    TotalDiscount = 15.00,
                    TotalTax = 31.50,
                    TotalSurcharge = 7.50,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetOrdersAsync_ShouldReturnLimitedNonDeletedOrders()
        {
            var orders = await _orderService.GetOrdersAsync(2);
            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders.All(o => !o.IsDeleted));
            Assert.AreEqual("ORD001", orders[0].Reference);
            Assert.AreEqual("ORD002", orders[1].Reference);
            Assert.IsTrue(orders[0].Stocks.Any(s => s.Quantity == 5));
            Assert.IsTrue(orders[1].Stocks.Any(s => s.Quantity == 10));
        }

        [TestMethod]
        public async Task GetAllOrdersAsync_ShouldReturnAllNonDeletedOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders.All(o => !o.IsDeleted));
            Assert.IsTrue(orders.Any(o => o.Reference == "ORD001"));
            Assert.IsTrue(orders.Any(o => o.Reference == "ORD002"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnOrder()
        {
            var order = await _orderService.GetByIdAsync(1);
            Assert.IsNotNull(order);
            Assert.AreEqual("ORD001", order.Reference);
            Assert.IsFalse(order.IsDeleted);
            Assert.AreEqual(1, order.Stocks.Count);
            Assert.AreEqual(5, order.Stocks[0].Quantity);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var order = await _orderService.GetByIdAsync(999);
            Assert.IsNull(order);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            var order = await _orderService.GetByIdAsync(3);
            Assert.IsNull(order);
        }

        [TestMethod]
        public async Task AddOrderAsync_ValidOrder_ShouldAddAndLog()
        {
            var newOrder = new Order
            {
                SourceId = 4,
                OrderDate = DateTime.UtcNow,
                RequestDate = DateTime.UtcNow.AddDays(1),
                Reference = "ORD004",
                Reference_extra = "Extra4",
                Order_status = "New",
                Notes = "Note4",
                ShippingNotes = "ShipNote4",
                PickingNotes = "PickNote4",
                WarehouseId = 4,
                ShipTo = "Client D",
                BillTo = "Client D",
                ShipmentId = 4,
                TotalAmount = 300.00,
                TotalDiscount = 30.00,
                TotalTax = 63.00,
                TotalSurcharge = 15.00,
                Stocks = new List<OrderStock>
                {
                    new OrderStock { ItemId = "3", Quantity = 8 }
                }
            };

            var addedOrder = await _orderService.AddOrderAsync(newOrder);
            Assert.IsNotNull(addedOrder);
            Assert.AreEqual("ORD004", addedOrder.Reference);
            Assert.IsFalse(addedOrder.IsDeleted);
            Assert.IsTrue(addedOrder.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedOrder.UpdatedAt > DateTime.MinValue);
            Assert.AreEqual(1, addedOrder.Stocks.Count);
            Assert.AreEqual(8, addedOrder.Stocks[0].Quantity);

            var dbOrder = await _dbContext.Orders.FindAsync(addedOrder.Id);
            Assert.IsNotNull(dbOrder);
            Assert.AreEqual("ORD004", dbOrder.Reference);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Order", "Create", "/api/v1/orders", $"Created order {addedOrder.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ExistingId_ShouldUpdateAndLog()
        {
            var updatedOrder = new Order
            {
                SourceId = 1,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                RequestDate = DateTime.UtcNow.AddDays(1),
                Reference = "ORD001-Updated",
                Reference_extra = "Extra1-Updated",
                Order_status = "Processing",
                Notes = "Updated Note",
                ShippingNotes = "Updated ShipNote",
                PickingNotes = "Updated PickNote",
                WarehouseId = 1,
                ShipTo = "Client A-Updated",
                BillTo = "Client A-Updated",
                ShipmentId = 1,
                TotalAmount = 150.75,
                TotalDiscount = 15.00,
                TotalTax = 31.50,
                TotalSurcharge = 7.50,
                Stocks = new List<OrderStock>
                {
                    new OrderStock { ItemId = "1", Quantity = 10 }
                }
            };

            var result = await _orderService.UpdateOrderAsync(1, updatedOrder);
            Assert.IsNotNull(result);
            Assert.AreEqual("ORD001-Updated", result.Reference);
            Assert.AreEqual("Processing", result.Order_status);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);
            Assert.AreEqual(1, result.Stocks.Count);
            Assert.AreEqual(10, result.Stocks[0].Quantity);

            var dbOrder = await _dbContext.Orders.Include(o => o.Stocks).FirstOrDefaultAsync(o => o.Id == 1);
            Assert.IsNotNull(dbOrder);
            Assert.AreEqual("ORD001-Updated", dbOrder.Reference);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Order", "Update", "/api/v1/orders/1", "Updated order 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateOrderAsync_NonExistingId_ShouldReturnNull()
        {
            var updatedOrder = new Order
            {
                SourceId = 1,
                OrderDate = DateTime.UtcNow,
                RequestDate = DateTime.UtcNow.AddDays(1),
                Reference = "ORD999",
                WarehouseId = 1,
                ShipmentId = 1,
                TotalAmount = 100.00
            };

            var result = await _orderService.UpdateOrderAsync(999, updatedOrder);
            Assert.IsNull(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            var result = await _orderService.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);
            var order = await _dbContext.Orders.FindAsync(1);
            Assert.IsNotNull(order);
            Assert.IsTrue(order.IsDeleted);
            Assert.IsTrue(order.UpdatedAt > order.CreatedAt);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Order", "Delete", "/api/v1/orders/1", "Soft deleted order 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _orderService.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            var result = await _orderService.SoftDeleteByIdAsync(3);
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}