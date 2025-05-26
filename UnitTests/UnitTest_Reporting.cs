using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using CargoHubV2;
using CargoHubV2.Models;
using CargoHubV2.Services;
using CargoHubV2.Data;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_Reporting
    {
        private CargoHubDbContext _dbContext;
        private ReportingService _reportingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase("TestCargoHubDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            SeedDatabase(_dbContext);
            _reportingService = new ReportingService(_dbContext);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Clients.AddRange(new List<Client>
            {
                new Client { ClientId = 1, Name = "Alice", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Client { ClientId = 2, Name = "Bob", CreatedAt = new DateTime(2023, 6, 1), IsDeleted = false },
                new Client { ClientId = 3, Name = "Charlie", CreatedAt = new DateTime(2023, 5, 1), IsDeleted = true } // deleted client
            });

            context.Warehouses.AddRange(new List<Warehouse>
            {
                new Warehouse { WarehouseId = 1, Name = "Warehouse A", CreatedAt = new DateTime(2023, 2, 1), IsDeleted = false },
                new Warehouse { WarehouseId = 2, Name = "Warehouse B", CreatedAt = new DateTime(2023, 7, 1), IsDeleted = false },
                new Warehouse { WarehouseId = 3, Name = "Warehouse C", CreatedAt = new DateTime(2023, 3, 1), IsDeleted = true }
            });

            context.Orders.AddRange(new List<Order>
            {
                new Order { Id = 1, CreatedAt = new DateTime(2023, 4, 1) },
                new Order { Id = 2, CreatedAt = new DateTime(2023, 8, 1) }
            });

            context.SaveChanges();
        }

        [TestMethod]
        public void GenerateReport_ReturnsClientsWithinDateRange()
        {
            var result = _reportingService.GenerateReport(
                "clients",
                new DateTime(2023, 1, 1),
                new DateTime(2023, 12, 31),
                null);

            Assert.IsNotNull(result);
            var clients = result.Cast<Client>().ToList();

            Assert.AreEqual(2, clients.Count); // only 2 non-deleted clients in range
            Assert.IsFalse(clients.Any(c => c.IsDeleted));
        }

        [TestMethod]
        public void GenerateReport_ReturnsWarehousesWithinDateRange_FilteredByWarehouseId()
        {
            var result = _reportingService.GenerateReport(
                "warehouses",
                new DateTime(2023, 1, 1),
                new DateTime(2023, 12, 31),
                2);

            Assert.IsNotNull(result);
            var warehouses = result.Cast<Warehouse>().ToList();

            Assert.AreEqual(1, warehouses.Count);
            Assert.AreEqual(2, warehouses[0].WarehouseId);
        }

        [TestMethod]
        public void GenerateReport_ReturnsAllWarehousesWithinDateRange_WhenNoWarehouseId()
        {
            var result = _reportingService.GenerateReport(
                "warehouses",
                new DateTime(2023, 1, 1),
                new DateTime(2023, 12, 31),
                null);

            Assert.IsNotNull(result);
            var warehouses = result.Cast<Warehouse>().ToList();

            Assert.AreEqual(2, warehouses.Count); // excludes deleted one
            Assert.IsFalse(warehouses.Any(w => w.IsDeleted));
        }

        [TestMethod]
        public void GenerateReport_ReturnsOrdersWithinDateRange()
        {
            var result = _reportingService.GenerateReport(
                "orders",
                new DateTime(2023, 1, 1),
                new DateTime(2023, 12, 31),
                null);

            Assert.IsNotNull(result);
            var orders = result.Cast<Order>().ToList();

            Assert.AreEqual(2, orders.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateReport_ThrowsExceptionForInvalidEntity()
        {
            _reportingService.GenerateReport(
                "invalid_entity",
                new DateTime(2023, 1, 1),
                new DateTime(2023, 12, 31),
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateReport_ThrowsExceptionForInvalidDateRange()
        {
            _reportingService.GenerateReport(
                "clients",
                new DateTime(2024, 1, 1),
                new DateTime(2023, 1, 1),
                null);
        }
    }
}
