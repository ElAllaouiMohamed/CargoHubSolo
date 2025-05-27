

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
    public class UnitTest_WarehousesController
    {
        private Mock<IWarehouseService>? _mockWarehouseService;
        private Mock<ILoggingService>? _mockLoggingService;
        private WarehousesController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockWarehouseService = new Mock<IWarehouseService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new WarehousesController(_mockWarehouseService.Object, _mockLoggingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithWarehouses()
        {
            var warehouses = new List<Warehouse> { new Warehouse { Id = 1, Code = "WH001" } };
            _mockWarehouseService!.Setup(s => s.GetWarehousesAsync(1)).ReturnsAsync(warehouses);

            var result = await _controller!.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(warehouses, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllWarehouses()
        {
            var warehouses = new List<Warehouse> { new Warehouse { Id = 2, Code = "WH002" } };
            _mockWarehouseService!.Setup(s => s.GetAllWarehousesAsync()).ReturnsAsync(warehouses);

            var result = await _controller!.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(warehouses, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithWarehouse()
        {
            var warehouse = new Warehouse { Id = 3, Code = "WH003" };
            _mockWarehouseService!.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(warehouse);


            var result = await _controller!.GetById(3);


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(warehouse, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {

            _mockWarehouseService!.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Warehouse?)null);


            var result = await _controller!.GetById(999);


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CheckHazardCompliance_Compliant_ReturnsOk()
        {

            _mockWarehouseService!.Setup(s => s.CheckHazardComplianceAsync(1))
                .ReturnsAsync((true, new List<Inventory>()));

            var result = await _controller!.CheckHazardCompliance(1);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var message = okResult.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value) as string;
            Assert.AreEqual("No forbidden items found", message);
        }

        [TestMethod]
        public async Task CheckHazardCompliance_NonCompliant_ReturnsBadRequest()
        {

            var forbiddenItems = new List<Inventory> { new Inventory { Id = 1 } };
            _mockWarehouseService!.Setup(s => s.CheckHazardComplianceAsync(1))
                .ReturnsAsync((false, forbiddenItems));


            var result = await _controller!.CheckHazardCompliance(1);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            var message = badRequestResult.Value?.GetType().GetProperty("message")?.GetValue(badRequestResult.Value) as string;
            var items = badRequestResult.Value?.GetType().GetProperty("items")?.GetValue(badRequestResult.Value) as List<Inventory>;
            Assert.AreEqual("Forbidden items detected", message);
            Assert.AreEqual(forbiddenItems, items);
        }

        [TestMethod]
        public async Task CheckHazardCompliance_NonExistingWarehouse_ReturnsNotFound()
        {

            _mockWarehouseService!.Setup(s => s.CheckHazardComplianceAsync(999))
                .ReturnsAsync((false, (List<Inventory>?)null));

            var result = await _controller!.CheckHazardCompliance(999);


            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidWarehouse_ReturnsCreatedAtAction()
        {

            var warehouse = new Warehouse { Code = "NewWH", Name = "New Warehouse" };
            var createdWarehouse = new Warehouse { Id = 4, Code = "NewWH", Name = "New Warehouse" };
            _mockWarehouseService!.Setup(s => s.AddWarehouseAsync(warehouse)).ReturnsAsync(createdWarehouse);
            _controller!.ModelState.Clear();

            var result = await _controller.Create(warehouse);


            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdWarehouse, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues!["id"]);
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Warehouse", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Warehouse created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {

            var warehouse = new Warehouse { Code = "" }; // Invalid (missing required fields)
            _controller!.ModelState.AddModelError("Name", "The Name field is required.");


            var result = await _controller.Create(warehouse);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingWarehouse_ReturnsOk()
        {
            var updatedWarehouse = new Warehouse { Code = "UpdatedWH", Name = "Updated Warehouse" };
            var resultWarehouse = new Warehouse { Id = 5, Code = "UpdatedWH", Name = "Updated Warehouse" };
            _mockWarehouseService!.Setup(s => s.UpdateWarehouseAsync(5, updatedWarehouse)).ReturnsAsync(resultWarehouse);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(5, updatedWarehouse);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultWarehouse, okResult.Value);
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Warehouse", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Warehouse 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingWarehouse_ReturnsNotFound()
        {

            var updatedWarehouse = new Warehouse { Code = "NonexistentWH", Name = "Nonexistent Warehouse" };
            _mockWarehouseService!.Setup(s => s.UpdateWarehouseAsync(999, updatedWarehouse)).ReturnsAsync((Warehouse?)null);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(999, updatedWarehouse);


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {

            var updatedWarehouse = new Warehouse { Code = "" }; // Invalid (missing required fields)
            _controller!.ModelState.AddModelError("Name", "The Name field is required.");


            var result = await _controller.Update(5, updatedWarehouse);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingWarehouse_ReturnsNoContent()
        {

            _mockWarehouseService!.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller!.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Warehouse", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Warehouse 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingWarehouse_ReturnsNotFound()
        {

            _mockWarehouseService!.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);


            var result = await _controller!.SoftDelete(999);


            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
