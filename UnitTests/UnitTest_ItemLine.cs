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
    public class UnitTest_ItemLineService
    {
        private CargoHubDbContext _dbContext;
        private Mock<ILoggingService> _mockLogging;
        private ItemLineService _itemLineService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLogging = new Mock<ILoggingService>();
            _itemLineService = new ItemLineService(_dbContext, _mockLogging.Object);
        }

        [TestMethod]
        public async Task AddItemLineAsync_ShouldAddItemLineAndLog()
        {
            var itemLine = new Item_Line
            {
                Name = "TestItemLine",
                Description = "Test description"
            };

            var result = await _itemLineService.AddItemLineAsync(itemLine);

            Assert.IsNotNull(result);
            Assert.AreEqual("TestItemLine", result.Name);
            Assert.IsTrue(result.Id != 0);

            _mockLogging.Verify(m => m.LogAsync("system", "Item_Line", "Create", "/api/v1/itemlines", It.IsAny<string>()), Times.Once);

            var dbItemLine = await _dbContext.ItemLines.FindAsync(result.Id);
            Assert.IsNotNull(dbItemLine);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnItemLine_WhenExists()
        {
            var itemLine = new Item_Line { Name = "existing", Description = "oldDesc" };
            _dbContext.ItemLines.Add(itemLine);
            await _dbContext.SaveChangesAsync();

            var result = await _itemLineService.GetByIdAsync(itemLine.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("existing", result.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _itemLineService.GetByIdAsync(-1);
            Assert.IsNull(result);
        }
        

        [TestMethod]
        public async Task UpdateItemLineAsync_ShouldUpdateAndLog_WhenExists()
        {
            var itemLine = new Item_Line { Name = "oldName", Description = "oldDesc" };
            _dbContext.ItemLines.Add(itemLine);
            await _dbContext.SaveChangesAsync();

            var updated = new Item_Line { Name = "newName", Description = "newDesc" };

            var result = await _itemLineService.UpdateItemLineAsync(itemLine.Id, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual("newName", result.Name);
            Assert.AreEqual("newDesc", result.Description);

            _mockLogging.Verify(m => m.LogAsync("system", "Item_Line", "Update", $"/api/v1/itemlines/{itemLine.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateItemLineAsync_ShouldReturnNull_WhenNotExists()
        {
            var updated = new Item_Line { Name = "newName", Description = "oldDesc" };
            var result = await _itemLineService.UpdateItemLineAsync(-1, updated);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldSoftDeleteAndLog_WhenExists()
        {
            var itemLine = new Item_Line { Name = "toDelete", Description = "oldDesc" };
            _dbContext.ItemLines.Add(itemLine);
            await _dbContext.SaveChangesAsync();

            var result = await _itemLineService.SoftDeleteByIdAsync(itemLine.Id);

            Assert.IsTrue(result);

            var deletedItemLine = await _dbContext.ItemLines.FindAsync(itemLine.Id);
            Assert.IsTrue(deletedItemLine.IsDeleted);

            _mockLogging.Verify(m => m.LogAsync("system", "Item_Line", "Delete", $"/api/v1/itemlines/{itemLine.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldReturnFalse_WhenNotExists()
        {
            var result = await _itemLineService.SoftDeleteByIdAsync(-1);
            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }
    }
}
