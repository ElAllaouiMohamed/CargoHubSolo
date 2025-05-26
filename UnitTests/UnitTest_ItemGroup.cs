using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using CargoHubV2.Data;
using CargoHubV2;
namespace UnitTests
{
    [TestClass]
    public class UnitTest_ItemGroup
    {
        private CargoHubDbContext _dbContext;
        private ItemGroupService _itemGroupService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCargoHubdata")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _itemGroupService = new ItemGroupService(_dbContext);
            SeedDatabase(_dbContext);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Items_Groups.AddRange(
                new Item_Group
                {
                    Id = 1,
                    Name = "dummy",
                    Description = "Dummy",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Group
                {
                    Id = 2,
                    Name = "dummy2",
                    Description = "Dummy2",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Group
                {
                    Id = 100,
                    Name = "dummy100",
                    Description = "Dummy100",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public void TestGetAll()
        {
            var itemGroups = _itemGroupService.GetItemGroupsAsync().Result.ToList();
            Assert.IsTrue(itemGroups.Count >= 1);
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(999, false)]
        public void TestGetById(int id, bool expectedResult)
        {
            var itemGroup = _itemGroupService.GetItemGroupByIdAsync(id).Result;
            Assert.AreEqual(expectedResult, itemGroup != null);
        }

        [TestMethod]
        [DataRow(15, "dummyDescription", true)]
        [DataRow(4, null, false)]
        public void TestPost(int id, string description, bool expectedResult)
        {
            var newItemGroup = new Item_Group
            {
                Id = id,
                Name = "dummy",
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var result = _itemGroupService.AddItemGroupAsync(newItemGroup).Result.returnedItemGroup;
            Assert.AreEqual(result != null, expectedResult);
        }

        [TestMethod]
        [DataRow(1, "updatedName", true)]
        [DataRow(1, null, false)]
        public void TestPut(int id, string name, bool expectedResult)
        {
            var updateGroup = new Item_Group
            {
                Id = id,
                Name = name,
                Description = "dummyUpdated",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var result = _itemGroupService.UpdateItemGroupAsync(id, updateGroup).Result.returnedItemGroup;
            Assert.AreEqual(result != null, expectedResult);
        }

        [TestMethod]
        [DataRow(100, true)]
        [DataRow(999999, false)]
        public void TestDelete(int id, bool expectedResult)
        {
            var result = _itemGroupService.DeleteItemGroupAsync(id).Result;
            Assert.AreEqual(result, expectedResult);
        }
    }
}
