using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using CargoHubV2.Data;
using CargoHubV2;
namespace UnitTests
{
    [TestClass]
    public class UnitTest_ShipmentService
    {
        private CargoHubDbContext _dbContext;
        private ShipmentService _shipmentService;
        private Mock<LoggingService> _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unieker DB per test run
                .Options;

            _dbContext = new CargoHubDbContext(options);
            SeedDatabase(_dbContext);

            _mockLoggingService = new Mock<LoggingService>();
            _mockLoggingService.Setup(l => l.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                               .Returns(Task.CompletedTask);

            _shipmentService = new ShipmentService(_dbContext, _mockLoggingService.Object);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Voeg Items, Warehouses, Shipments toe (vereist voor Shipment tests)
            context.Items.AddRange(
                new Item { Uid = "P000001", Code = "Item1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Uid = "P000002", Code = "Item2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            context.Warehouses.Add(new Warehouse
            {
                WarehouseId = 1,
                Code = "WH001",
                Name = "Main Warehouse",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            context.Shipments.Add(new Shipment
            {
                Id = 1,
                WarehouseId = 1,
                ShipmentStatus = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Stocks = new List<ShipmentStock> {
                    new ShipmentStock { ItemId = "P000001", Quantity = 10 },
                    new ShipmentStock { ItemId = "P000002", Quantity = 5 }
                }
            });

            context.SaveChanges();
        }

        [TestMethod]
        public async Task TestGetShipmentsAsync_ReturnsCorrectCount()
        {
            var result = await _shipmentService.GetShipmentsAsync(10);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task TestGetAllShipmentsAsync_ReturnsAll()
        {
            var result = await _shipmentService.GetAllShipmentsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task TestGetByIdAsync_ReturnsShipment()
        {
            var shipment = await _shipmentService.GetByIdAsync(1);
            Assert.IsNotNull(shipment);
            Assert.AreEqual(1, shipment.Id);
            Assert.AreEqual(2, shipment.Stocks.Count);
        }

        [TestMethod]
        public async Task TestAddShipmentAsync_AddsAndLogs()
        {
            var newShipment = new Shipment
            {
                WarehouseId = 1,
                ShipmentStatus = "New",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Stocks = new List<ShipmentStock> {
                    new ShipmentStock { ItemId = "P000001", Quantity = 3 }
                }
            };

            var result = await _shipmentService.AddShipmentAsync(newShipment);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Id > 0);

            _mockLoggingService.Verify(l => l.LogAsync("system", "Shipment", "Create", "/api/v1/shipments", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task TestUpdateShipmentAsync_UpdatesAndLogs()
        {
            var updatedShipment = new Shipment
            {
                Id = 1,
                WarehouseId = 1,
                ShipmentStatus = "Updated",
                Stocks = new List<ShipmentStock> {
                    new ShipmentStock { ItemId = "P000001", Quantity = 15 }
                }
            };

            var result = await _shipmentService.UpdateShipmentAsync(1, updatedShipment);
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated", result.ShipmentStatus);
            Assert.AreEqual(1, result.Stocks.Count);

            _mockLoggingService.Verify(l => l.LogAsync("system", "Shipment", "Update", "/api/v1/shipments/1", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task TestUpdateShipmentAsync_ReturnsNull_WhenNotFound()
        {
            var updatedShipment = new Shipment { Id = 999 };
            var result = await _shipmentService.UpdateShipmentAsync(999, updatedShipment);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TestSoftDeleteByIdAsync_DeletesAndLogs()
        {
            var result = await _shipmentService.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);

            var deletedShipment = await _shipmentService.GetByIdAsync(1);
            Assert.IsNull(deletedShipment);

            _mockLoggingService.Verify(l => l.LogAsync("system", "Shipment", "Delete", "/api/v1/shipments/1", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task TestSoftDeleteByIdAsync_ReturnsFalse_WhenNotFound()
        {
            var result = await _shipmentService.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);
        }
    }
}
