using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_Location
    {
        private CargoHubDbContext _dbContext;
        private LocationService _locationService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestLocationDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            SeedDatabase(_dbContext);
            _locationService = new LocationService(_dbContext);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Items_Groups.AddRange(
                new Item_Group { Id = 1, Name = "dummy", Description = "Dummy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Item_Group { Id = 2, Name = "dummy2", Description = "Dummy2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            context.Items_Types.AddRange(
                new Item_Type { Id = 1, Name = "dummy", Description = "Dummy", ItemLineId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Item_Type { Id = 2, Name = "dummy2", Description = "Dummy2", ItemLineId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            context.Items_Lines.AddRange(
                new Item_Line { Id = 1, Name = "dummy", Description = "Dummy", ItemGroupId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Item_Line { Id = 2, Name = "dummy2", Description = "Dummy2", ItemGroupId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            context.Items.AddRange(
                new Item {
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
                new Item {
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

            context.Warehouses.AddRange(
                new Warehouse
                {
                    Id = 1,
                    Code = "WH001",
                    Name = "Main Warehouse",
                    Address = "123 Warehouse St.",
                    Zip = "12345",
                    City = "Sample City",
                    Province = "Sample Province",
                    Country = "Sample Country",
                    Contact = new Contact { Name = "John Doe", Phone = "555-1234", Email = "johndoe@example.com" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Locations.AddRange(
                new Location {
                    Id = 1,
                    Name = "Row: A, Rack: 1, Shelf: 1",
                    Code = "LOC001",
                    WarehouseId = 1,
                    ItemAmounts = new Dictionary<string, int>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Location {
                    Id = 2,
                    Name = "Row: B, Rack: 2, Shelf: 2",
                    Code = "LOC002",
                    WarehouseId = 1,
                    ItemAmounts = new Dictionary<string, int>(),
                    MaxHeight = 100,
                    MaxWidth = 20,
                    MaxDepth = 20,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Location {
                    Id = 3,
                    Name = "Row: C, Rack: 3, Shelf: 3",
                    Code = "LOC003",
                    WarehouseId = 2,
                    ItemAmounts = new Dictionary<string, int>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.Inventories.AddRange(
                new Inventory {
                    Id = 1,
                    ItemId = "P000001",
                    Description = "dummy",
                    ItemReference = "dummy",
                    TotalOnHand = 100,
                    TotalExpected = 1,
                    TotalOrdered = 1,
                    TotalAllocated = 1,
                    TotalAvailable = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Inventory {
                    Id = 2,
                    ItemId = "P000002",
                    Description = "dummy2",
                    ItemReference = "dummy2",
                    TotalOnHand = 100,
                    TotalExpected = 1,
                    TotalOrdered = 1,
                    TotalAllocated = 1,
                    TotalAvailable = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task TestGetLocationById()
        {
            var location = await _locationService.GetLocationAsync(1);
            Assert.IsNotNull(location);
            Assert.AreEqual("Row: A, Rack: 1, Shelf: 1", location.Name);
        }

        [TestMethod]
        public async Task TestGetLocationById_NotFound()
        {
            var location = await _locationService.GetLocationAsync(999);
            Assert.IsNull(location);
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(999999, false)]
        public async Task TestGetLocationByWarehouse(int id, bool expectedResult)
        {
            var locations = await _locationService.GetLocationsByWarehouseAsync(id);
            Assert.AreEqual(locations.Count() > 0, expectedResult);
        }

        [TestMethod]
        public async Task TestGetAllLocations()
        {
            var locations = await _locationService.GetLocationsAsync();
            Assert.AreEqual(3, locations.Count());
        }

        [TestMethod]
        [DataRow("Row: Z, Rack: 150, Shelf: 5", false)]
        [DataRow("Row: A, Rack: 5, Shelf: 11", false)]
        [DataRow("Row: AA, Rack: 5, Shelf: 5", false)]
        [DataRow("Row: 1, Rack: 5, Shelf: 5", false)]
        [DataRow("Row: a, Rack: 5, Shelf: 5", false)]
        [DataRow("Row: A, Rack: 5, Shelf: 5", true)]
        public async Task TestLocationNameValidation(string name, bool expectedResult)
        {
            var isValid = await _locationService.IsValidLocationNameAsync(name);
            Assert.AreEqual(expectedResult, isValid);
        }

        [TestMethod]
        [DataRow(100, 1, true)]
        [DataRow(100, 100, false)]
        [DataRow(1, 1, false)]
        public async Task TestAddLocation(int id, int warehouseId, bool expectedResult)
        {
            var testLocation = new Location
            {
                Id = id,
                Name = "Row: G, Rack: 5, Shelf: 8",
                Code = "LOC100",
                ItemAmounts = new Dictionary<string, int>(),
                WarehouseId = warehouseId,
                MaxHeight = 20,
                MaxWidth = 20,
                MaxDepth = 20,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var returnedLocation = await _locationService.AddLocationAsync(testLocation);
            Assert.AreEqual(returnedLocation != null, expectedResult);
        }

        [TestMethod]
        public async Task TestUpdateLocation_ValidData()
        {
            var location = await _dbContext.Locations.FirstOrDefaultAsync(x => x.Id == 1);
            location.Name = "Row: D, Rack: 4, Shelf: 4";
            location.Code = "LOC001-Updated";
            location.MaxHeight = 20;
            location.MaxWidth = 20;
            location.MaxDepth = 20;
            var updatedLocation = await _locationService.UpdateLocationAsync(1, location);
            Assert.IsNotNull(updatedLocation);
            Assert.AreEqual("Row: D, Rack: 4, Shelf: 4", updatedLocation.Name);
        }

        [TestMethod]
        public async Task TestUpdateLocationItems_ValidData()
        {
            int id = 1;
            List<LocationItem> locationItems = new List<LocationItem>
            {
                new LocationItem { ItemId = "P000001", Amount = 15, Classification = "None", Height = 10, Depth = 10, Width = 10 },
                new LocationItem { ItemId = "P000002", Amount = 15, Classification = "None", Height = 10, Depth = 10, Width = 10 }
            };
            var updatedLocation = await _locationService.UpdateLocationItemsAsync(id, locationItems);
            Assert.IsNotNull(updatedLocation);
            if (updatedLocation != null)
            {
                foreach (var item in locationItems)
                {
                    Assert.IsTrue(updatedLocation.ItemAmounts.ContainsKey(item.ItemId) && updatedLocation.ItemAmounts[item.ItemId] == item.Amount);
                }
            }
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(999999, false)]
        public async Task TestUpdateLocationItems_InvalidItemOrLocationId(int id, bool expectedResult)
        {
            List<LocationItem> locationItems = new List<LocationItem>
            {
                new LocationItem { ItemId = "P000001", Amount = 15, Classification = "None", Height = 10, Depth = 10, Width = 10 },
                new LocationItem { ItemId = "P999999", Amount = 15, Classification = "None", Height = 10, Depth = 10, Width = 10 }
            };
            var updatedLocation = await _locationService.UpdateLocationItemsAsync(id, locationItems);
            Assert.AreEqual(updatedLocation != null, expectedResult);
            if (updatedLocation != null)
            {
                Assert.AreEqual(1, updatedLocation.ItemAmounts.Count);
            }
        }

        [TestMethod]
        [DataRow(10000, 10, 10)]
        [DataRow(10, 10000, 10)]
        [DataRow(10, 10, 10000)]
        public async Task TestUpdateLocationItems_TooHighSize(int height, int depth, int width)
        {
            int id = 2;
            List<LocationItem> locationItems = new List<LocationItem>
            {
                new LocationItem { ItemId = "P000002", Amount = 15, Classification = "None", Height = 10, Depth = 10, Width = 10 },
                new LocationItem { ItemId = "P000002", Amount = 15, Classification = "None", Height = height, Depth = depth, Width = width }
            };
            var updatedLocation = await _locationService.UpdateLocationItemsAsync(id, locationItems);
            Assert.IsNotNull(updatedLocation);
            Assert.AreEqual(1, updatedLocation.ItemAmounts.Count);
        }

        [TestMethod]
        public async Task TestUpdateLocationItems_RestrictedCategory()
        {
            int id = 1;
            List<LocationItem> locationItems = new List<LocationItem>
            {
                new LocationItem { ItemId = "P000001", Amount = 15, Classification = "DummyRestricted", Height = 10, Depth = 10, Width = 10 }
            };
            var updatedLocation = await _locationService.UpdateLocationItemsAsync(id, locationItems);
            Assert.IsNotNull(updatedLocation);
            Assert.AreEqual(0, updatedLocation.ItemAmounts.Count);
        }

        [TestMethod]
        public async Task TestUpdateLocation_NotFound()
        {
            var location = await _dbContext.Locations.FirstOrDefaultAsync(x => x.Id == 1);
            location.Name = "Row: E, Rack: 5, Shelf: 5";
            location.Code = "LOC999";
            location.WarehouseId = 4;
            var updatedLocation = await _locationService.UpdateLocationAsync(999, location);
            Assert.IsNull(updatedLocation);
        }

        [TestMethod]
        public async Task TestDeleteLocation_ValidId()
        {
            var result = await _locationService.DeleteLocationAsync(1);
            Assert.IsTrue(result);

            var location = await _locationService.GetLocationAsync(1);
            Assert.IsNull(location);
        }

        [TestMethod]
        public async Task TestDeleteLocation_InvalidId()
        {
            var result = await _locationService.DeleteLocationAsync(999);
            Assert.IsFalse(result);
        }
    }
}
