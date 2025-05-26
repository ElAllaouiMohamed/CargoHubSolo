using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using CargoHubV2.Data;
using CargoHubV2;
namespace UnitTests
{
    [TestClass]
    public class UnitTest_InventoryService
    {
        private CargoHubDbContext _dbContext;
        private InventoryService _inventoryService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCargoHubDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _inventoryService = new InventoryService(_dbContext);
            SeedDatabase(_dbContext);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.ItemGroups.Add(new Item_Group
            {
                Id = 1,
                Name = "dummy",
                Description = "Dummy",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.ItemGroups.Add(new Item_Group
            {
                Id = 2,
                Name = "dummy2",
                Description = "Dummy2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.ItemTypes.Add(new Item_Type
            {
                Id = 1,
                Name = "dummy",
                Description = "Dummy",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.ItemTypes.Add(new Item_Type
            {
                Id = 2,
                Name = "dummy2",
                Description = "Dummy2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.ItemLines.Add(new Item_Line
            {
                Id = 1,
                Name = "dummy",
                Description = "Dummy",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.ItemLines.Add(new Item_Line
            {
                Id = 2,
                Name = "dummy2",
                Description = "Dummy2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.Items.Add(new Item
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
            });

            context.Items.Add(new Item
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
            });

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

            context.Inventories.Add(new Inventory
            {
                Id = 1,
                ItemId = "P000001",
                Description = "dummy",
                ItemReference = "dummy",
                Locations = new List<int> { 1 },
                TotalOnHand = 1,
                TotalExpected = 1,
                TotalOrdered = 1,
                TotalAllocated = 1,
                TotalAvailable = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.Locations.Add(new Location
            {
                Id = 1,
                Name = "Row: A, Rack: 1, Shelf: 1",
                Code = "LOC001",
                WarehouseId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            context.SaveChanges();
        }

        [TestMethod]
        public async Task TestGetAllInventories()
        {
            var inventories = await _inventoryService.GetInventoriesAsync();
            Assert.IsTrue(inventories.Count() >= 1);
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(999, false)]
        public async Task TestGetInventoryById(int inventoryId, bool expectedResult)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            Assert.AreEqual(expectedResult, inventory != null);
        }

        [TestMethod]
        public async Task TestAddInventory()
        {
            var newInventory = new Inventory
            {
                ItemId = "P000001",
                Description = "New dummy",
                ItemReference = "NewRef",
                Locations = new List<int> { 1 },
                TotalOnHand = 5,
                TotalExpected = 5,
                TotalOrdered = 5,
                TotalAllocated = 5,
                TotalAvailable = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var addedInventory = await _inventoryService.AddInventoryAsync(newInventory);
            Assert.IsNotNull(addedInventory);
        }

        [TestMethod]
        public async Task TestUpdateInventory()
        {
            var inventoryToUpdate = await _inventoryService.GetInventoryByIdAsync(1);
            if (inventoryToUpdate != null)
            {
                inventoryToUpdate.Description = "Updated dummy";
                var updatedInventory = await _inventoryService.UpdateInventoryAsync(1, inventoryToUpdate);
                Assert.IsNotNull(updatedInventory);
                Assert.AreEqual("Updated dummy", updatedInventory.Description);
            }
            else
            {
                Assert.Fail("Inventory to update not found.");
            }
        }

        [TestMethod]
        public async Task TestDeleteInventory()
        {
            var deleteResult = await _inventoryService.DeleteInventoryAsync(1);
            Assert.IsTrue(deleteResult);

            var deletedInventory = await _inventoryService.GetInventoryByIdAsync(1);
            Assert.IsNull(deletedInventory);
        }
    }
}
