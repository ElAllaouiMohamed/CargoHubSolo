using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargohubV2.Controllers;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_ShipmentsController
    {
        private Mock<IShipmentService> _mockShipmentService;
        private Mock<ILoggingService> _mockLoggingService;
        private ShipmentsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockShipmentService = new Mock<IShipmentService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new ShipmentsController(_mockShipmentService.Object, _mockLoggingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithShipments()
        {
            var shipments = new List<Shipment> { new Shipment { Id = 1, OrderId = 100 } };
            _mockShipmentService.Setup(s => s.GetShipmentsAsync(1)).ReturnsAsync(shipments);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(shipments, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllShipments()
        {
            var shipments = new List<Shipment> { new Shipment { Id = 2, OrderId = 101 } };
            _mockShipmentService.Setup(s => s.GetAllShipmentsAsync()).ReturnsAsync(shipments);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(shipments, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithShipment()
        {
            var shipment = new Shipment { Id = 3, OrderId = 102 };
            _mockShipmentService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(shipment);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(shipment, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockShipmentService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Shipment)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidShipment_ReturnsCreatedAtAction()
        {
            var shipment = new Shipment { OrderId = 103, SourceId = 1 };
            var createdShipment = new Shipment { Id = 4, OrderId = 103, SourceId = 1 };
            _mockShipmentService.Setup(s => s.AddShipmentAsync(shipment)).ReturnsAsync(createdShipment);
            _controller.ModelState.Clear();

            var result = await _controller.Create(shipment);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdShipment, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Shipment", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Shipment created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var shipment = new Shipment { OrderId = 0 }; // Invalid (missing required fields)
            _controller.ModelState.AddModelError("OrderId", "The OrderId field is required.");

            var result = await _controller.Create(shipment);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingShipment_ReturnsOk()
        {
            var updatedShipment = new Shipment { OrderId = 104, SourceId = 1 };
            var resultShipment = new Shipment { Id = 5, OrderId = 104, SourceId = 1 };
            _mockShipmentService.Setup(s => s.UpdateShipmentAsync(5, updatedShipment)).ReturnsAsync(resultShipment);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedShipment);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultShipment, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Shipment", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Shipment 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingShipment_ReturnsNotFound()
        {
            var updatedShipment = new Shipment { OrderId = 105, SourceId = 1 };
            _mockShipmentService.Setup(s => s.UpdateShipmentAsync(999, updatedShipment)).ReturnsAsync((Shipment)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedShipment);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedShipment = new Shipment { OrderId = 0 }; // Invalid (missing required fields)
            _controller.ModelState.AddModelError("OrderId", "The OrderId field is required.");

            var result = await _controller.Update(5, updatedShipment);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingShipment_ReturnsNoContent()
        {
            _mockShipmentService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Shipment", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Shipment 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingShipment_ReturnsNotFound()
        {
            _mockShipmentService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
