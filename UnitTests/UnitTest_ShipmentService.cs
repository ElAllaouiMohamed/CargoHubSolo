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
    public class ShipmentServiceTests
    {
        private CargoHubDbContext? _dbContext;
        private ShipmentService? _shipmentService;
        private Mock<ILoggingService>? _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<ILoggingService>();
            _shipmentService = new ShipmentService(_dbContext, _mockLoggingService.Object);
            SeedDatabase(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Shipments.AddRange(
                new Shipment
                {
                    Id = 1,
                    OrderId = 1,
                    SourceId = 1,
                    OrderDate = "2025-05-25",
                    RequestDate = "2025-05-26",
                    ShipmentDate = "2025-05-27",
                    ShipmentType = "Standard",
                    ShipmentStatus = "Pending",
                    Notes = "Note1",
                    CarrierCode = "DHL",
                    CarrierDescription = "DHL Express",
                    ServiceCode = "EXP",
                    PaymentType = "Prepaid",
                    TransferMode = "Road",
                    TotalPackageCount = 5,
                    TotalPackageWeight = 25.5,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    Stocks = new List<ShipmentStock>
                    {
                        new ShipmentStock { Id = 1, ShipmentId = 1, ItemId = "1", Quantity = 10 }
                    }
                },
                new Shipment
                {
                    Id = 2,
                    OrderId = 2,
                    SourceId = 2,
                    OrderDate = "2025-05-24",
                    RequestDate = "2025-05-25",
                    ShipmentDate = "2025-05-26",
                    ShipmentType = "Express",
                    ShipmentStatus = "Shipped",
                    Notes = "Note2",
                    CarrierCode = "UPS",
                    CarrierDescription = "UPS Standard",
                    ServiceCode = "STD",
                    PaymentType = "COD",
                    TransferMode = "Air",
                    TotalPackageCount = 3,
                    TotalPackageWeight = 15.75,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    Stocks = new List<ShipmentStock>
                    {
                        new ShipmentStock { Id = 2, ShipmentId = 2, ItemId = "2", Quantity = 5 }
                    }
                },
                new Shipment
                {
                    Id = 3,
                    OrderId = 3,
                    SourceId = 3,
                    OrderDate = "2025-05-23",
                    RequestDate = "2025-05-24",
                    ShipmentDate = "2025-05-25",
                    ShipmentType = "Standard",
                    ShipmentStatus = "Cancelled",
                    Notes = "Note3",
                    CarrierCode = "FEDEX",
                    CarrierDescription = "FedEx Ground",
                    ServiceCode = "GND",
                    PaymentType = "Prepaid",
                    TransferMode = "Road",
                    TotalPackageCount = 2,
                    TotalPackageWeight = 10.0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetShipmentsAsync_ShouldReturnLimitedNonDeletedShipments()
        {
            var shipments = await _shipmentService!.GetShipmentsAsync(2);
            Assert.AreEqual(2, shipments.Count);
            Assert.IsTrue(shipments.All(s => !s.IsDeleted));
            Assert.AreEqual("Pending", shipments[0].ShipmentStatus);
            Assert.AreEqual("Shipped", shipments[1].ShipmentStatus);
            Assert.IsTrue(shipments[0].Stocks.Any(s => s.Quantity == 10));
            Assert.IsTrue(shipments[1].Stocks.Any(s => s.Quantity == 5));
        }

        [TestMethod]
        public async Task GetAllShipmentsAsync_ShouldReturnAllNonDeletedShipments()
        {
            var shipments = await _shipmentService!.GetAllShipmentsAsync();
            Assert.AreEqual(2, shipments.Count);
            Assert.IsTrue(shipments.All(s => !s.IsDeleted));
            Assert.IsTrue(shipments.Any(s => s.ShipmentStatus == "Pending"));
            Assert.IsTrue(shipments.Any(s => s.ShipmentStatus == "Shipped"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnShipment()
        {
            var shipment = await _shipmentService!.GetByIdAsync(1);
            Assert.IsNotNull(shipment);
            Assert.AreEqual("Pending", shipment.ShipmentStatus);
            Assert.IsFalse(shipment.IsDeleted);
            Assert.AreEqual(1, shipment.Stocks.Count);
            Assert.AreEqual(10, shipment.Stocks[0].Quantity);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var shipment = await _shipmentService!.GetByIdAsync(999);
            Assert.IsNull(shipment);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            var shipment = await _shipmentService!.GetByIdAsync(3);
            Assert.IsNull(shipment);
        }

        [TestMethod]
        public async Task AddShipmentAsync_ValidShipment_ShouldAddAndLog()
        {
            var newShipment = new Shipment
            {
                OrderId = 4,
                SourceId = 4,
                OrderDate = "2025-05-28",
                RequestDate = "2025-05-29",
                ShipmentDate = "2025-05-30",
                ShipmentType = "Express",
                ShipmentStatus = "New",
                Notes = "Note4",
                CarrierCode = "DHL",
                CarrierDescription = "DHL Express",
                ServiceCode = "EXP",
                PaymentType = "Prepaid",
                TransferMode = "Air",
                TotalPackageCount = 4,
                TotalPackageWeight = 20.0,
                Stocks = new List<ShipmentStock>
                {
                    new ShipmentStock { ItemId = "3", Quantity = 15 }
                }
            };

            var addedShipment = await _shipmentService!.AddShipmentAsync(newShipment);
            Assert.IsNotNull(addedShipment);
            Assert.AreEqual("New", addedShipment.ShipmentStatus);
            Assert.IsFalse(addedShipment.IsDeleted);
            Assert.IsTrue(addedShipment.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedShipment.UpdatedAt > DateTime.MinValue);
            Assert.AreEqual(1, addedShipment.Stocks.Count);
            Assert.AreEqual(15, addedShipment.Stocks[0].Quantity);

            var dbShipment = await _dbContext!.Shipments.FindAsync(addedShipment.Id);
            Assert.IsNotNull(dbShipment);
            Assert.AreEqual("New", dbShipment.ShipmentStatus);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Shipment", "Create", "/api/v1/shipments", $"Created shipment {addedShipment.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateShipmentAsync_ExistingId_ShouldUpdateAndLog()
        {
            var updatedShipment = new Shipment
            {
                OrderId = 1,
                SourceId = 1,
                OrderDate = "2025-05-26",
                RequestDate = "2025-05-27",
                ShipmentDate = "2025-05-28",
                ShipmentType = "Standard",
                ShipmentStatus = "Processing",
                Notes = "Updated Note",
                CarrierCode = "UPS",
                CarrierDescription = "UPS Standard",
                ServiceCode = "STD",
                PaymentType = "COD",
                TransferMode = "Road",
                TotalPackageCount = 6,
                TotalPackageWeight = 30.0,
                Stocks = new List<ShipmentStock>
                {
                    new ShipmentStock { ItemId = "1", Quantity = 20 }
                }
            };

            var result = await _shipmentService!.UpdateShipmentAsync(1, updatedShipment);
            Assert.IsNotNull(result);
            Assert.AreEqual("Processing", result.ShipmentStatus);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);
            Assert.AreEqual(1, result.Stocks.Count);
            Assert.AreEqual(20, result.Stocks[0].Quantity);

            var dbShipment = await _dbContext!.Shipments.Include(s => s.Stocks).FirstOrDefaultAsync(s => s.Id == 1);
            Assert.IsNotNull(dbShipment);
            Assert.AreEqual("Processing", dbShipment.ShipmentStatus);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Shipment", "Update", "/api/v1/shipments/1", "Updated shipment 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateShipmentAsync_NonExistingId_ShouldReturnNull()
        {
            var updatedShipment = new Shipment
            {
                OrderId = 999,
                SourceId = 999,
                OrderDate = "2025-05-28",
                RequestDate = "2025-05-29",
                ShipmentDate = "2025-05-30",
                ShipmentStatus = "New",
                TotalPackageCount = 1,
                TotalPackageWeight = 5.0
            };

            var result = await _shipmentService!.UpdateShipmentAsync(999, updatedShipment);
            Assert.IsNull(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            var result = await _shipmentService!.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);
            var shipment = await _dbContext!.Shipments.FindAsync(1);
            Assert.IsNotNull(shipment);
            Assert.IsTrue(shipment.IsDeleted);
            Assert.IsTrue(shipment.UpdatedAt > shipment.CreatedAt);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Shipment", "Delete", "/api/v1/shipments/1", "Soft deleted shipment 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _shipmentService!.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            var result = await _shipmentService!.SoftDeleteByIdAsync(3);
            Assert.IsFalse(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}