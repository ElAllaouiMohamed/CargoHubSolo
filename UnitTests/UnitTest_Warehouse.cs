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
    public class WarehouseServiceTests
    {
        private CargoHubDbContext? _dbContext;
        private WarehouseService? _warehouseService;
        private Mock<ILoggingService>? _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<ILoggingService>();
            _warehouseService = new WarehouseService(_dbContext, _mockLoggingService.Object);
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
            context.Warehouses.AddRange(
                new Warehouse
                {
                    Id = 1,
                    Code = "WH001",
                    Name = "Warehouse A",
                    Address = "123 Main St",
                    Zip = "1012 AB",
                    City = "Amsterdam",
                    Province = "Noord-Holland",
                    Country = "Netherlands",
                    Contact = new Contact { Name = "John Doe", Phone = "+31201234567", Email = "john.doe@example.com" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Warehouse
                {
                    Id = 2,
                    Code = "WH002",
                    Name = "Warehouse B",
                    Address = "456 High St",
                    Zip = "1000",
                    City = "Brussels",
                    Province = "Brussels",
                    Country = "Belgium",
                    Contact = new Contact { Name = "Jane Smith", Phone = "+3229876543", Email = "jane.smith@example.com" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Warehouse
                {
                    Id = 3,
                    Code = "WH003",
                    Name = "Warehouse C",
                    Address = "789 Broad St",
                    Zip = "10115",
                    City = "Berlin",
                    Province = "Berlin",
                    Country = "Germany",
                    Contact = new Contact { Name = "Max Mustermann", Phone = "+49301234567", Email = "max.mustermann@example.com" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetWarehousesAsync_ShouldReturnLimitedNonDeletedWarehouses()
        {
            var warehouses = await _warehouseService!.GetWarehousesAsync(2);
            Assert.AreEqual(2, warehouses.Count);
            Assert.IsTrue(warehouses.All(w => !w.IsDeleted));
            Assert.AreEqual("Warehouse A", warehouses[0].Name);
            Assert.AreEqual("Warehouse B", warehouses[1].Name);
        }

        [TestMethod]
        public async Task GetAllWarehousesAsync_ShouldReturnAllNonDeletedWarehouses()
        {
            var warehouses = await _warehouseService!.GetAllWarehousesAsync();
            Assert.AreEqual(2, warehouses.Count);
            Assert.IsTrue(warehouses.All(w => !w.IsDeleted));
            Assert.IsTrue(warehouses.Any(w => w.Name == "Warehouse A"));
            Assert.IsTrue(warehouses.Any(w => w.Name == "Warehouse B"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnWarehouse()
        {
            var warehouse = await _warehouseService!.GetByIdAsync(1);
            Assert.IsNotNull(warehouse);
            Assert.AreEqual("Warehouse A", warehouse.Name);
            Assert.IsFalse(warehouse.IsDeleted);
            Assert.AreEqual("John Doe", warehouse.Contact.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var warehouse = await _warehouseService!.GetByIdAsync(999);
            Assert.IsNull(warehouse);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            var warehouse = await _warehouseService!.GetByIdAsync(3);
            Assert.IsNull(warehouse);
        }

        [TestMethod]
        public async Task AddWarehouseAsync_ValidWarehouse_ShouldAddAndLog()
        {
            var newWarehouse = new Warehouse
            {
                Code = "WH004",
                Name = "Warehouse D",
                Address = "101 Park Ave",
                Zip = "75001",
                City = "Paris",
                Province = "Île-de-France",
                Country = "France",
                Contact = new Contact { Name = "Alice Dupont", Phone = "+33123456789", Email = "alice.dupont@example.com" }
            };

            var addedWarehouse = await _warehouseService!.AddWarehouseAsync(newWarehouse);
            Assert.IsNotNull(addedWarehouse);
            Assert.AreEqual("Warehouse D", addedWarehouse.Name);
            Assert.IsFalse(addedWarehouse.IsDeleted);
            Assert.IsTrue(addedWarehouse.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedWarehouse.UpdatedAt > DateTime.MinValue);
            Assert.AreEqual("Alice Dupont", addedWarehouse.Contact.Name);

            var dbWarehouse = await _dbContext!.Warehouses.FindAsync(addedWarehouse.Id);
            Assert.IsNotNull(dbWarehouse);
            Assert.AreEqual("Warehouse D", dbWarehouse.Name);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Warehouse", "Create", "/api/v1/warehouses", $"Created warehouse {addedWarehouse.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateWarehouseAsync_ExistingId_ShouldUpdateAndLog()
        {
            var updatedWarehouse = new Warehouse
            {
                Code = "WH001-Updated",
                Name = "Warehouse A-Updated",
                Address = "123 Updated St",
                Zip = "3012 CD",
                City = "Rotterdam",
                Province = "Zuid-Holland",
                Country = "Netherlands",
                Contact = new Contact { Name = "Bob Johnson", Phone = "+31109876543", Email = "bob.johnson@example.com" }
            };

            var result = await _warehouseService!.UpdateWarehouseAsync(1, updatedWarehouse);
            Assert.IsNotNull(result);
            Assert.AreEqual("Warehouse A-Updated", result.Name);
            Assert.AreEqual("WH001-Updated", result.Code);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);
            Assert.AreEqual("Bob Johnson", result.Contact.Name);

            var dbWarehouse = await _dbContext!.Warehouses.FindAsync(1);
            Assert.IsNotNull(dbWarehouse);
            Assert.AreEqual("Warehouse A-Updated", dbWarehouse.Name);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Warehouse", "Update", "/api/v1/warehouses/1", "Updated warehouse 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateWarehouseAsync_NonExistingId_ShouldReturnNull()
        {
            var updatedWarehouse = new Warehouse
            {
                Code = "WH999",
                Name = "Warehouse X",
                Address = "999 Unknown St",
                Zip = "0000",
                City = "Unknown",
                Province = "Unknown",
                Country = "Unknown",
                Contact = new Contact { Name = "Unknown", Phone = "+9999999999", Email = "unknown@example.com" }
            };

            var result = await _warehouseService!.UpdateWarehouseAsync(999, updatedWarehouse);
            Assert.IsNull(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            var result = await _warehouseService!.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);
            var warehouse = await _dbContext!.Warehouses.FindAsync(1);
            Assert.IsNotNull(warehouse);
            Assert.IsTrue(warehouse.IsDeleted);
            Assert.IsTrue(warehouse.UpdatedAt > warehouse.CreatedAt);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Warehouse", "Delete", "/api/v1/warehouses/1", "Soft deleted warehouse 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _warehouseService!.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            var result = await _warehouseService!.SoftDeleteByIdAsync(3);
            Assert.IsFalse(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}