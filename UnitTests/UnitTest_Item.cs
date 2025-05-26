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
    public class UnitTest_ItemService
    {
        private CargoHubDbContext _dbContext;
        private Mock<ILoggingService> _mockLogging;
        private ItemService _itemService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLogging = new Mock<ILoggingService>();
            _itemService = new ItemService(_dbContext, _mockLogging.Object);
        }

        [TestMethod]
        public async Task AddItemAsync_ShouldAddItemAndLog()
        {
            var item = new Item
            {
                UId = "uid123",
                Code = "codeABC",
                Description = "Test item"
            };

            var result = await _itemService.AddItemAsync(item);

            Assert.IsNotNull(result);
            Assert.AreEqual("uid123", result.UId);
            Assert.IsTrue(result.Id != 0);

            _mockLogging.Verify(m => m.LogAsync("system", "Item", "Create", "/api/v1/items", It.IsAny<string>()), Times.Once);

            var dbItem = await _dbContext.Items.FindAsync(result.Id);
            Assert.IsNotNull(dbItem);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _itemService.GetByIdAsync(-1);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateItemAsync_ShouldUpdateAndLog_WhenExists()
        {
            var item = new Item { UId = "oldUid", Description = "oldDesc" };
            _dbContext.Items.Add(item);
            await _dbContext.SaveChangesAsync();

            var updated = new Item { UId = "newUid", Description = "newDesc" };

            var result = await _itemService.UpdateItemAsync(item.Id, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual("newUid", result.UId);
            Assert.AreEqual("newDesc", result.Description);

            _mockLogging.Verify(m => m.LogAsync("system", "Item", "Update", $"/api/v1/items/{item.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateItemAsync_ShouldReturnNull_WhenNotExists()
        {
            var updated = new Item { UId = "newUid" };
            var result = await _itemService.UpdateItemAsync(-1, updated);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldSoftDeleteAndLog_WhenExists()
        {
            var item = new Item { UId = "toDelete" };
            _dbContext.Items.Add(item);
            await _dbContext.SaveChangesAsync();

            var result = await _itemService.SoftDeleteByIdAsync(item.Id);

            Assert.IsTrue(result);

            var deletedItem = await _dbContext.Items.FindAsync(item.Id);
            Assert.IsTrue(deletedItem.IsDeleted);

            _mockLogging.Verify(m => m.LogAsync("system", "Item", "Delete", $"/api/v1/items/{item.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldReturnFalse_WhenNotExists()
        {
            var result = await _itemService.SoftDeleteByIdAsync(-1);
            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }
    }
}
