using System;
using System.Collections.Generic;
using System.Linq;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;


            _dbContext = new CargoHubDbContext(options);
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    WarehouseId = 123,
                    TotalAmount = 100.5,
                    CreatedAt = new DateTime(2025, 1, 15),
                    Stocks = new List<OrderStock>
                    {
                        new OrderStock { Quantity = 5 },
                        new OrderStock { Quantity = 3 }
                    }
                },
                new Order
                {
                    Id = 2,
                    WarehouseId = 123,
                    TotalAmount = 200.75,
                    CreatedAt = new DateTime(2025, 1, 20),
                    Stocks = new List<OrderStock>
                    {
                        new OrderStock { Quantity = 2 }
                    }
                }
            };

            _dbContext.Orders.AddRange(orders);
            _dbContext.SaveChanges();

            _reportingService = new ReportingService(_dbContext);
        }

        [TestMethod]
        public void TestGetWarehouseReport_ReturnsCorrectTotals()
        {
            var result = _reportingService.GetWarehouseReport(123);

            Assert.AreEqual(2, result.TotalOrders);
            Assert.AreEqual(10, result.TotalItems); // 5+3+2=10
        }
        [TestMethod]
        public void TestGetRevenueBetweenDates_ReturnsCorrectSum()
        {
            var start = new DateTime(2025, 1, 10);
            var end = new DateTime(2025, 1, 30);

            var revenue = _reportingService.GetRevenueBetweenDates(start, end);

            Assert.AreEqual(301.25, revenue.TotalRevenue);
        }
    }
}
