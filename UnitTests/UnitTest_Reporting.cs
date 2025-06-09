using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using CargohubV2.Controllers;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_Reporting
    {
        private CargoHubDbContext _context = null!;
        private ReportingService _reportingService = null!;
        private ReportingController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new CargoHubDbContext(options);
            SeedTestData(_context);

            _reportingService = new ReportingService(_context);
            _controller = new ReportingController(_reportingService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        private void SeedTestData(CargoHubDbContext context)
        {
            var now = DateTime.UtcNow;

            context.Orders.AddRange(
                new Order
                {
                    SourceId = 1,
                    OrderDate = new DateTime(2023, 1, 10),
                    RequestDate = new DateTime(2023, 1, 11),
                    Reference = "RPT-001",
                    Reference_extra = "",
                    Order_status = "",
                    Notes = "",
                    ShippingNotes = "",
                    PickingNotes = "",
                    ShipTo = "",
                    BillTo = "",
                    WarehouseId = 1,
                    ShipmentId = 1,
                    TotalAmount = 150.0,
                    CreatedAt = now, // Gebruik CreatedAt voor revenue test
                    UpdatedAt = now,
                    IsDeleted = false,
                    Stocks = new List<OrderStock>
                    {
                        new OrderStock { ItemId = "A1", Quantity = 5 }
                    }
                },
                new Order
                {
                    SourceId = 1,
                    OrderDate = new DateTime(2023, 2, 15),
                    RequestDate = new DateTime(2023, 2, 16),
                    Reference = "RPT-002",
                    Reference_extra = "",
                    Order_status = "",
                    Notes = "",
                    ShippingNotes = "",
                    PickingNotes = "",
                    ShipTo = "",
                    BillTo = "",
                    WarehouseId = 1,
                    ShipmentId = 2,
                    TotalAmount = 250.0,
                    CreatedAt = now, // Gebruik CreatedAt voor revenue test
                    UpdatedAt = now,
                    IsDeleted = false,
                    Stocks = new List<OrderStock>
                    {
                        new OrderStock { ItemId = "B2", Quantity = 10 }
                    }
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public void TestGetWarehouseReport_ReturnsCorrectTotals()
        {
            var result = _controller.GetWarehouseReport(1) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var report = result.Value as ReportResult;
            Assert.IsNotNull(report);
            Assert.AreEqual(2, report.TotalOrders);
            Assert.AreEqual(15, report.TotalItems); // 5 + 10
        }

        [TestMethod]
        public void TestGetRevenueBetweenDates_ReturnsCorrectTotal()
        {
            var start = DateTime.UtcNow.AddDays(-1); // Binnen de CreatedAt-periode
            var end = DateTime.UtcNow.AddDays(1);

            var result = _controller.GetRevenue(start, end) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var revenue = result.Value as RevenueResult;
            Assert.IsNotNull(revenue);
            Assert.AreEqual(400.0, revenue.TotalRevenue); // 150 + 250
        }

    }
}
