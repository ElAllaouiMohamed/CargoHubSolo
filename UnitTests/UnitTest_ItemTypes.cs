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
    public class UnitTest_ItemTypes
    {
        private CargoHubDbContext _dbContext;
        private ItemTypeService _itemTypeService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCargoHubDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _itemTypeService = new ItemTypeService(_dbContext);
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
                }
            );

            context.Items_Lines.AddRange(
                new Item_Line
                {
                    Id = 1,
                    Name = "dummy",
                    Description = "Dummy",
                    ItemGroupId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Line
                {
                    Id = 2,
                    Name = "dummy2",
                    Description = "Dummy2",
                    ItemGroupId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Item_Types.AddRange(
                new Item_Type
                {
                    Id = 1,
                    Name = "dummy",
                    Description = "Dummy",
                    ItemLineId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Type
                {
                    Id = 2,
                    Name = "dummy2",
                    Description = "Dummy2",
                    ItemLineId = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item_Type
                {
                    Id = 100,
                    Name = "dummy100",
                    Description = "Dummy100",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Items.AddRange(
                new Item
                {
                    Id = 1,
                    UId = "P000001",
                    Code = "Dummy",
                    Description = "dummy",
                    ShortDescription = "dummy",
                    UpcCode = "null",
                    ModelNumber = "null",
                    CommodityCode = "null",
                    ItemLineId = 1,
                    ItemGroupId = 1,
                    ItemTypeId = 1,
                    UnitPurchaseQuantity = 1,
                    UnitOrderQuantity = 1,
                    PackOrderQuantity = 1,
                    SupplierId = 1,
                    SupplierCode = "null",
                    SupplierPartNumber = "null",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Item
                {
                    Id = 2,
                    UId = "P000002",
                    Code = "Dummy2",
                    Description = "dummy2",
                    ShortDescription = "dummy2",
                    UpcCode = "null",
                    ModelNumber = "null",
                    CommodityCode = "null",
                    ItemLineId = 2,
                    ItemGroupId = 2,
                    ItemTypeId = 2,
                    UnitPurchaseQuantity = 1,
                    UnitOrderQuantity = 1,
                    PackOrderQuantity = 1,
                    SupplierId = 2,
                    SupplierCode = "null",
                    SupplierPartNumber = "null",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Inventories.Add(new Inventory
            {
                Id = 1,
                ItemId = "P000001",
                Description = "dummy",
                ItemReference = "dummy",
                TotalOnHand = 1,
                TotalExpected = 1,
                TotalOrdered = 1,
                TotalAllocated = 1,
                TotalAvailable = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.SaveChanges();
        }

        [TestMethod]
        public void TestGetAll()
        {
            var itemTypes = _itemTypeService.GetItemTypesAsync().Result.ToList();
            Assert.IsTrue(itemTypes.Count >= 1);
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(999, false)]
        public void TestGetById(int id, bool expectedResult)
        {
            var itemType = _itemTypeService.GetItemTypeByIdAsync(id).Result;
            Assert.AreEqual(expectedResult, itemType != null);
        }

        [TestMethod]
        [DataRow(25, "dummy", true)]
        [DataRow(200, null, false)]
        public void TestPost(int id, string description, bool expectedResult)
        {
            var newItemType = new Item_Type
            {
                Id = id,
                Name = "dummy",
                Description = description,
                ItemLineId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var result = _itemTypeService.AddItemTypeAsync(newItemType).Result.returnedItemType;
            Assert.AreEqual(result != null, expectedResult);
        }

        [TestMethod]
        [DataRow(1, "updatedName", true)]
        [DataRow(1, null, false)]
        public void TestPut(int id, string name, bool expectedResult)
        {
            var updateItemType = new Item_Type
            {
                Id = id,
                Name = name,
                Description = "dummyUpdated",
                ItemLineId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var result = _itemTypeService.UpdateItemTypeAsync(id, updateItemType).Result.returnedItemType;
            Assert.AreEqual(result != null, expectedResult);
        }

        [TestMethod]
        [DataRow(100, true)]
        [DataRow(999999, false)]
        public void TestDelete(int id, bool expectedResult)
        {
            var result = _itemTypeService.DeleteItemTypeAsync(id).Result;
            Assert.AreEqual(result, expectedResult);
        }
    }
}
