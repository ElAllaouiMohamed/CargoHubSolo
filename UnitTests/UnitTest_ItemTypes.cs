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
    public class ItemTypeServiceTests
    {
        private CargoHubDbContext _dbContext;
        private ItemTypeService _itemTypeService;
        private Mock<ILoggingService> _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<ILoggingService>();
            _itemTypeService = new ItemTypeService(_dbContext, _mockLoggingService.Object);
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
            context.ItemTypes.AddRange(
                new Item_Type
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "Portable computers",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Type
                {
                    Id = 2,
                    Name = "Smartphone",
                    Description = "Mobile phones",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Type
                {
                    Id = 3,
                    Name = "Tablet",
                    Description = "Tablet devices",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetItemTypesAsync_ShouldReturnLimitedNonDeletedItemTypes()
        {
            var itemTypes = await _itemTypeService.GetItemTypesAsync(2);
            Assert.AreEqual(2, itemTypes.Count);
            Assert.IsTrue(itemTypes.All(it => !it.IsDeleted));
            Assert.AreEqual("Laptop", itemTypes[0].Name);
            Assert.AreEqual("Smartphone", itemTypes[1].Name);
        }

        [TestMethod]
        public async Task GetAllItemTypesAsync_ShouldReturnAllNonDeletedItemTypes()
        {
            var itemTypes = await _itemTypeService.GetAllItemTypesAsync();
            Assert.AreEqual(2, itemTypes.Count);
            Assert.IsTrue(itemTypes.All(it => !it.IsDeleted));
            Assert.IsTrue(itemTypes.Any(it => it.Name == "Laptop"));
            Assert.IsTrue(itemTypes.Any(it => it.Name == "Smartphone"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnItemType()
        {
            var itemType = await _itemTypeService.GetByIdAsync(1);
            Assert.IsNotNull(itemType);
            Assert.AreEqual("Laptop", itemType.Name);
            Assert.IsFalse(itemType.IsDeleted);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var itemType = await _itemTypeService.GetByIdAsync(999);
            Assert.IsNull(itemType);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            var itemType = await _itemTypeService.GetByIdAsync(3);
            Assert.IsNull(itemType);
        }

        [TestMethod]
        public async Task AddItemTypeAsync_ValidItemType_ShouldAddAndLog()
        {
            var newItemType = new Item_Type
            {
                Name = "Headphones",
                Description = "Audio devices"
            };

            var addedItemType = await _itemTypeService.AddItemTypeAsync(newItemType);
            Assert.IsNotNull(addedItemType);
            Assert.AreEqual("Headphones", addedItemType.Name);
            Assert.IsFalse(addedItemType.IsDeleted);
            Assert.IsTrue(addedItemType.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedItemType.UpdatedAt > DateTime.MinValue);

            var dbItemType = await _dbContext.ItemTypes.FindAsync(addedItemType.Id);
            Assert.IsNotNull(dbItemType);
            Assert.AreEqual("Headphones", dbItemType.Name);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Item_Type", "Create", "/api/v1/itemtypes", $"Created item_type {addedItemType.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateItemTypeAsync_ExistingId_ShouldUpdateAndLog()
        {
            var updatedItemType = new Item_Type
            {
                Name = "Updated Laptop",
                Description = "Updated portable computers"
            };

            var result = await _itemTypeService.UpdateItemTypeAsync(1, updatedItemType);
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Laptop", result.Name);
            Assert.AreEqual("Updated portable computers", result.Description);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);

            var dbItemType = await _dbContext.ItemTypes.FindAsync(1);
            Assert.IsNotNull(dbItemType);
            Assert.AreEqual("Updated Laptop", dbItemType.Name);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Item_Type", "Update", "/api/v1/itemtypes/1", "Updated item_type 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateItemTypeAsync_NonExistingId_ShouldReturnNull()
        {
            var updatedItemType = new Item_Type
            {
                Name = "Updated Laptop",
                Description = "Updated portable computers"
            };

            var result = await _itemTypeService.UpdateItemTypeAsync(999, updatedItemType);
            Assert.IsNull(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            var result = await _itemTypeService.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);
            var itemType = await _dbContext.ItemTypes.FindAsync(1);
            Assert.IsNotNull(itemType);
            Assert.IsTrue(itemType.IsDeleted);
            Assert.IsTrue(itemType.UpdatedAt > itemType.CreatedAt);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Item_Type", "Delete", "/api/v1/itemtypes/1", "Soft deleted item_type 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _itemTypeService.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            var result = await _itemTypeService.SoftDeleteByIdAsync(3);
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}
