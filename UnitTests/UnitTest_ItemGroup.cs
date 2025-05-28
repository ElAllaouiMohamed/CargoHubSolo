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
    public class ItemGroupServiceTests
    {
        private CargoHubDbContext _dbContext;
        private ItemGroupService _itemGroupService;
        private Mock<ILoggingService> _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {

            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<ILoggingService>();
            _itemGroupService = new ItemGroupService(_dbContext, _mockLoggingService.Object);
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
            context.ItemGroups.AddRange(
                new Item_Group
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Group
                {
                    Id = 2,
                    Name = "Clothing",
                    Description = "Apparel and fashion items",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Group
                {
                    Id = 3,
                    Name = "Furniture",
                    Description = "Home and office furniture",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true // Soft-deleted
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetItemGroupsAsync_ShouldReturnLimitedNonDeletedItemGroups()
        {
            // Act
            var itemGroups = await _itemGroupService.GetItemGroupsAsync(2);

            // Assert
            Assert.AreEqual(2, itemGroups.Count);
            Assert.IsTrue(itemGroups.All(ig => !ig.IsDeleted));
            Assert.AreEqual("Electronics", itemGroups[0].Name);
            Assert.AreEqual("Clothing", itemGroups[1].Name);
        }

        [TestMethod]
        public async Task GetAllItemGroupsAsync_ShouldReturnAllNonDeletedItemGroups()
        {
            // Act
            var itemGroups = await _itemGroupService.GetAllItemGroupsAsync();

            // Assert
            Assert.AreEqual(2, itemGroups.Count);
            Assert.IsTrue(itemGroups.All(ig => !ig.IsDeleted));
            Assert.IsTrue(itemGroups.Any(ig => ig.Name == "Electronics"));
            Assert.IsTrue(itemGroups.Any(ig => ig.Name == "Clothing"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnItemGroup()
        {
            // Act
            var itemGroup = await _itemGroupService.GetByIdAsync(1);

            // Assert
            Assert.IsNotNull(itemGroup);
            Assert.AreEqual("Electronics", itemGroup.Name);
            Assert.IsFalse(itemGroup.IsDeleted);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Act
            var itemGroup = await _itemGroupService.GetByIdAsync(999);

            // Assert
            Assert.IsNull(itemGroup);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            // Act
            var itemGroup = await _itemGroupService.GetByIdAsync(3);

            // Assert
            Assert.IsNull(itemGroup);
        }




        

        [TestMethod]
        public async Task AddItemGroupAsync_ValidItemGroup_ShouldAddAndLog()
        {
            // Arrange
            var newItemGroup = new Item_Group
            {
                Name = "Books",
                Description = "Books and literature"
            };

            // Act
            var addedItemGroup = await _itemGroupService.AddItemGroupAsync(newItemGroup);

            // Assert
            Assert.IsNotNull(addedItemGroup);
            Assert.AreEqual("Books", addedItemGroup.Name);
            Assert.IsFalse(addedItemGroup.IsDeleted);
            Assert.IsTrue(addedItemGroup.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedItemGroup.UpdatedAt > DateTime.MinValue);

            var dbItemGroup = await _dbContext.ItemGroups.FindAsync(addedItemGroup.Id);
            Assert.IsNotNull(dbItemGroup);
            Assert.AreEqual("Books", dbItemGroup.Name);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Item_Group", "Create", "/api/v1/itemgroups", $"Created item_group {addedItemGroup.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task AddItemGroupAsync_InvalidName_ShouldStillAdd()
        {
            // Arrange
            var newItemGroup = new Item_Group
            {
                Name = "Books123", // Ongeldig vanwege regex, maar service valideert niet
                Description = "Books and literature"
            };

            // Act
            var addedItemGroup = await _itemGroupService.AddItemGroupAsync(newItemGroup);

            // Assert
            Assert.IsNotNull(addedItemGroup);
            Assert.AreEqual("Books123", addedItemGroup.Name); // Service voert geen validatie uit
        }

        [TestMethod]
        public async Task UpdateItemGroupAsync_ExistingId_ShouldUpdateAndLog()
        {
            // Arrange
            var updatedItemGroup = new Item_Group
            {
                Name = "Updated Electronics",
                Description = "Updated electronic devices"
            };

            // Act
            var result = await _itemGroupService.UpdateItemGroupAsync(1, updatedItemGroup);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Electronics", result.Name);
            Assert.AreEqual("Updated electronic devices", result.Description);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);

            var dbItemGroup = await _dbContext.ItemGroups.FindAsync(1);
            Assert.IsNotNull(dbItemGroup);
            Assert.AreEqual("Updated Electronics", dbItemGroup.Name);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Item_Group", "Update", "/api/v1/itemgroups/1", "Updated item_group 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateItemGroupAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            var updatedItemGroup = new Item_Group
            {
                Name = "Updated Electronics",
                Description = "Updated electronic devices"
            };

            // Act
            var result = await _itemGroupService.UpdateItemGroupAsync(999, updatedItemGroup);

            // Assert
            Assert.IsNull(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            // Act
            var result = await _itemGroupService.SoftDeleteByIdAsync(1);

            // Assert
            Assert.IsTrue(result);
            var itemGroup = await _dbContext.ItemGroups.FindAsync(1);
            Assert.IsNotNull(itemGroup);
            Assert.IsTrue(itemGroup.IsDeleted);
            Assert.IsTrue(itemGroup.UpdatedAt > itemGroup.CreatedAt);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Item_Group", "Delete", "/api/v1/itemgroups/1", "Soft deleted item_group 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _itemGroupService.SoftDeleteByIdAsync(999);

            // Assert
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            // Act
            var result = await _itemGroupService.SoftDeleteByIdAsync(3);

            // Assert
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}
