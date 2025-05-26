using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CargohubV2.Models;
using CargohubV2.Contexts;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_InventoryService
    {
        private CargoHubDbContext _dbContext;
        private Mock<LoggingService> _mockLogging;
        private InventoryService _inventoryService;

        [TestInitialize]
        public void Setup()
        {
            // InMemory DB instellen
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CargoHubDbContext(options);

            _mockLogging = new Mock<LoggingService>();

            _inventoryService = new InventoryService(_dbContext, _mockLogging.Object);
        }

        [TestMethod]
        public async Task AddInventoryAsync_ShouldAddInventoryAndLog()
        {
            var inventory = new Inventory
            {
                ItemId = "item123",
                Description = "Test inventory",
                TotalOnHand = 10
            };

            var result = await _inventoryService.AddInventoryAsync(inventory);

            Assert.IsNotNull(result);
            Assert.AreEqual("item123", result.ItemId);
            Assert.IsTrue(result.Id != 0); // Id gegenereerd door InMemory DB

            // Check of LogAsync 1x werd aangeroepen
            _mockLogging.Verify(m => m.LogAsync("system", "Inventory", "Create", "/api/v1/inventories", It.IsAny<string>()), Times.Once);

            // Check dat het object ook in DB zit
            var dbInv = await _dbContext.Inventories.FindAsync(result.Id);
            Assert.IsNotNull(dbInv);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnInventory_WhenExists()
        {
            var inventory = new Inventory { ItemId = "id1", Description = "desc" };
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();

            var result = await _inventoryService.GetByIdAsync(inventory.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("id1", result.ItemId);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _inventoryService.GetByIdAsync(-1);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateInventoryAsync_ShouldUpdateAndLog_WhenExists()
        {
            var inventory = new Inventory { ItemId = "idOld", Description = "descOld" };
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();

            var updated = new Inventory { ItemId = "idNew", Description = "descNew", TotalOnHand = 5 };

            var result = await _inventoryService.UpdateInventoryAsync(inventory.Id, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual("idNew", result.ItemId);
            Assert.AreEqual("descNew", result.Description);
            Assert.AreEqual(5, result.TotalOnHand);

            _mockLogging.Verify(m => m.LogAsync("system", "Inventory", "Update", $"/api/v1/inventories/{inventory.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateInventoryAsync_ShouldReturnNull_WhenNotExists()
        {
            var updated = new Inventory { ItemId = "idNew" };
            var result = await _inventoryService.UpdateInventoryAsync(-1, updated);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldSoftDeleteAndLog_WhenExists()
        {
            var inventory = new Inventory { ItemId = "id1" };
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();

            var result = await _inventoryService.SoftDeleteByIdAsync(inventory.Id);

            Assert.IsTrue(result);

            var deletedInventory = await _dbContext.Inventories.FindAsync(inventory.Id);
            Assert.IsTrue(deletedInventory.IsDeleted);

            _mockLogging.Verify(m => m.LogAsync("system", "Inventory", "Delete", $"/api/v1/inventories/{inventory.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldReturnFalse_WhenNotExists()
        {
            var result = await _inventoryService.SoftDeleteByIdAsync(-1);
            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }
    }
}
