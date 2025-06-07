using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
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
        private Mock<ILoggingService> _mockLogging;
        private InventoryService _inventoryService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLogging = new Mock<ILoggingService>();
            _inventoryService = new InventoryService(_dbContext, _mockLogging.Object);
        }

        private Inventory CreateValidInventory()
        {
            return new Inventory
            {
                ItemId = "item123",
                Description = "Test inventory",
                ItemReference = "ref-001",
                Locations = new List<int> { 1 },
                TotalOnHand = 10,
                TotalExpected = 5,
                TotalOrdered = 3,
                TotalAllocated = 2,
                TotalAvailable = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                InventoryLocations = new List<InventoryLocation>()
            };
        }

        [TestMethod]
        public async Task AddInventoryAsync_ShouldAddInventoryAndLog()
        {
            var inventory = CreateValidInventory();

            var result = await _inventoryService.AddInventoryAsync(inventory);

            Assert.IsNotNull(result);
            Assert.AreEqual("item123", result.ItemId);
            Assert.IsTrue(result.Id != 0);

            _mockLogging.Verify(m => m.LogAsync("system", "Inventory", "Create", "/api/v1/inventories", It.IsAny<string>()), Times.Once);

            var dbInv = await _dbContext.Inventories.FindAsync(result.Id);
            Assert.IsNotNull(dbInv);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnInventory_WhenExists()
        {
            var inventory = CreateValidInventory();
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();

            var result = await _inventoryService.GetByIdAsync(inventory.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("item123", result.ItemId);
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
            var inventory = CreateValidInventory();
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();

            var updated = CreateValidInventory();
            updated.ItemId = "item999";
            updated.Description = "Updated desc";
            updated.TotalOnHand = 99;

            var result = await _inventoryService.UpdateInventoryAsync(inventory.Id, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual("item999", result.ItemId);
            Assert.AreEqual("Updated desc", result.Description);
            Assert.AreEqual(99, result.TotalOnHand);

            _mockLogging.Verify(m => m.LogAsync("system", "Inventory", "Update", $"/api/v1/inventories/{inventory.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateInventoryAsync_ShouldReturnNull_WhenNotExists()
        {
            var updated = CreateValidInventory();
            var result = await _inventoryService.UpdateInventoryAsync(-1, updated);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldSoftDeleteAndLog_WhenExists()
        {
            var inventory = CreateValidInventory();
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();

            var result = await _inventoryService.SoftDeleteByIdAsync(inventory.Id);

            Assert.IsTrue(result);

            var deleted = await _dbContext.Inventories.FindAsync(inventory.Id);
            Assert.IsTrue(deleted.IsDeleted);

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
