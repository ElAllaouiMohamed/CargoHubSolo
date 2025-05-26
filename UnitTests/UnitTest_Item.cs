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
    public class UnitTest_Item
    {
        private CargoHubDbContext _dbContext;
        private ItemService _itemService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCargoHubDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _itemService = new ItemService(_dbContext);
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
                }
            );

            context.Item_Lines.AddRange(
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

            context.Items.AddRange(
                new Item
                {
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

            context.Warehouses.Add(new Warehouse
            {
                Id = 1,
                Code = "WH001",
                Name = "Main Warehouse",
                Address = "123 Warehouse St.",
                Zip = "12345",
                City = "Sample City",
                Province = "Sample Province",
                Country = "Sample Country",
                Contact = new Contact
                {
                    Name = "John Doe",
                    Phone = "555-1234",
                    Email = "johndoe@example.com"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.Locations.Add(new Location
            {
                Id = 1,
                WarehouseId = 1,
                Code = "LOC001",
                Name = "Aisle 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.Inventories.Add(new Inventory
            {
                Id = 1,
                ItemId = "P000001",
                Description = "dummy",
                ItemReference = "dummy",
                Locations = new List<int> { 1 },
                TotalOnHand = 100,
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
            var items = _itemService.GetItemsAsync().Result.ToList();
            Assert.IsTrue(items.Count >= 1);
        }

        [TestMethod]
        [DataRow("P000001", true)]
        [DataRow("P999999", false)]
        public void TestGetById(string id, bool expectedResult)
        {
            var item = _itemService.GetItemByIdAsync(id).Result;
            Assert.AreEqual(expectedResult, item != null);
        }

        [TestMethod]
        [DataRow("P000001", 1, true)]
        [DataRow("P000001", 4, false)]
        [DataRow("P999999", 1, false)]
        public void TestGetAmountByLocationId(string itemId, int locationId, bool expectedResult)
        {
            var amount = _itemService.GetItemAmountAtLocationByIdAsync(itemId, locationId).Result;
            Assert.AreEqual(amount != -1, expectedResult);
        }

        [TestMethod]
        [DataRow("P000100", "dummyCode", true)]
        [DataRow("P000015", null, false)]
        public void TestPost(string itemId, string commodityCode, bool expectedResult)
        {
            var newItem = new Item
            {
                UId = itemId,
                Code = "TestCode",
                Description = "TestDesc",
                ShortDescription = "TestShortDesc",
                UpcCode = "TestUpc",
                ModelNumber = "TestModel",
                CommodityCode = commodityCode,
                ItemLineId = 1,
                ItemGroupId = 1,
                ItemTypeId = 1,
                UnitPurchaseQuantity = 1,
                UnitOrderQuantity = 1,
                PackOrderQuantity = 1,
                SupplierId = 2,
                SupplierCode = "TestSupplierCode",
                SupplierPartNumber = "TestSupplierPart",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var result = _itemService.AddItemAsync(newItem).Result;
            Assert.AreEqual(result.returnedItem != null, expectedResult);
        }

        [TestMethod]
        [DataRow("P000001", "UpdatedCommodityCode", true)]
        [DataRow("P000001", null, false)]
        public void TestPut(string itemId, string commodityCode, bool expectedResult)
        {
            var updateItem = new Item
            {
                UId = itemId,
                Code = "UpdatedCode",
                Description = "UpdatedDesc",
                ShortDescription = "UpdatedShortDesc",
                UpcCode = "UpdatedUpc",
                ModelNumber = "UpdatedModel",
                CommodityCode = commodityCode,
                ItemLineId = 1,
                ItemGroupId = 1,
                ItemTypeId = 1,
                UnitPurchaseQuantity = 1,
                UnitOrderQuantity = 1,
                PackOrderQuantity = 1,
                SupplierId = 2,
                SupplierCode = "UpdatedSupplierCode",
                SupplierPartNumber = "UpdatedSupplierPart",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var result = _itemService.UpdateItemAsync(itemId, updateItem).Result.returnedItem;
            Assert.AreEqual(result != null, expectedResult);
        }

        [TestMethod]
        [DataRow("P000001", true)]
        [DataRow("P999999", false)]
        public void TestDelete(string itemId, bool expectedResult)
        {
            var result = _itemService.DeleteItemAsync(itemId).Result;
            Assert.AreEqual(result, expectedResult);
        }
    }
}
