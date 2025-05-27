using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using CargohubV2.Controllers;
using CargohubV2.Services;
using CargohubV2.Models;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_ReportingController
    {
        private Mock<IReportingService>? _mockReportingService;
        private ReportingController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockReportingService = new Mock<IReportingService>();
            _controller = new ReportingController(_mockReportingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public void GetWarehouseReport_ReturnsOkResult_WithReport()
        {
            int warehouseId = 1;
            var expectedReport = new ReportResult { TotalOrders = 10, TotalItems = 100 };
            _mockReportingService!.Setup(s => s.GetWarehouseReport(warehouseId)).Returns(expectedReport);

            var result = _controller!.GetWarehouseReport(warehouseId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(expectedReport, okResult.Value);
        }

        [TestMethod]
        public void GetOrdersCsv_ReturnsFileResult_WithCorrectContent()
        {
            int warehouseId = 2;
            string csvContent = "Order ID, Warehouse ID, Order Price\n1,2,100.00\n";
            _mockReportingService!.Setup(s => s.GenerateCsvReport(warehouseId)).Returns(csvContent);

            var result = _controller!.GetOrdersCsv(warehouseId);

            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = (FileContentResult)result;
            string fileContentString = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.AreEqual(csvContent, fileContentString);
            Assert.AreEqual("text/csv", fileResult.ContentType);
            Assert.AreEqual($"orders_warehouse_{warehouseId}.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public void GetRevenue_ReturnsOkResult_WithRevenue()
        {
            DateTime start = new DateTime(2023, 1, 1);
            DateTime end = new DateTime(2023, 12, 31);
            var expectedRevenue = new RevenueResult { TotalRevenue = 12345.67 };
            _mockReportingService!.Setup(s => s.GetRevenueBetweenDates(start, end)).Returns(expectedRevenue);

            var result = _controller!.GetRevenue(start, end);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(expectedRevenue, okResult.Value);
        }
    }
}
