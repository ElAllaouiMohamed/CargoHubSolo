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
    public class UnitTest_InventoriesController
    {
        private Mock<IInventoryService> _mockInventoryService;
        private Mock<ILoggingService> _mockLoggingService;
        private InventoriesController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockInventoryService = new Mock<IInventoryService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new InventoriesController(_mockInventoryService.Object, _mockLoggingService.Object);

            // HttpContext mocken zodat logging geen nullref veroorzaakt
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithInventories()
        {
            var inventories = new List<Inventory> { new Inventory { Id = 1, ItemId = "Item1" } };
            _mockInventoryService.Setup(s => s.GetInventoriesAsync(1)).ReturnsAsync(inventories);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(inventories, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllInventories()
        {
            var inventories = new List<Inventory> { new Inventory { Id = 2, ItemId = "Item2" } };
            _mockInventoryService.Setup(s => s.GetAllInventoriesAsync()).ReturnsAsync(inventories);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(inventories, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithInventory()
        {
            var inventory = new Inventory { Id = 3, ItemId = "Item3" };
            _mockInventoryService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(inventory);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(inventory, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockInventoryService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Inventory)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetInventoryLocations_ExistingInventoryId_ReturnsOkWithLocations()
        {
            var inventoryLocations = new List<InventoryLocation>
            {
                new InventoryLocation { Id = 1, InventoryId = 1, LocationId = 1 }
            };
            _mockInventoryService.Setup(s => s.GetInventoryLocationsAsync(1)).ReturnsAsync(inventoryLocations);

            var result = await _controller.GetInventoryLocations(1);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(inventoryLocations, okResult.Value);
        }

        [TestMethod]
        public async Task Create_ValidInventory_ReturnsCreatedAtAction()
        {
            var inventory = new Inventory { ItemId = "NewItem", Description = "Test" };
            var createdInventory = new Inventory { Id = 4, ItemId = "NewItem", Description = "Test" };
            _mockInventoryService.Setup(s => s.AddInventoryAsync(inventory)).ReturnsAsync(createdInventory);
            _controller.ModelState.Clear();

            var result = await _controller.Create(inventory);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdInventory, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Inventory", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Inventory created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var inventory = new Inventory { ItemId = "" }; // Invalid (empty ItemId)
            _controller.ModelState.AddModelError("ItemId", "The ItemId field is required.");

            var result = await _controller.Create(inventory);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingInventory_ReturnsOk()
        {
            var updatedInventory = new Inventory { ItemId = "UpdatedItem", Description = "Updated" };
            var resultInventory = new Inventory { Id = 5, ItemId = "UpdatedItem", Description = "Updated" };
            _mockInventoryService.Setup(s => s.UpdateInventoryAsync(5, updatedInventory)).ReturnsAsync(resultInventory);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedInventory);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultInventory, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Inventory", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Inventory 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingInventory_ReturnsNotFound()
        {
            var updatedInventory = new Inventory { ItemId = "NonexistentItem", Description = "Test" };
            _mockInventoryService.Setup(s => s.UpdateInventoryAsync(999, updatedInventory)).ReturnsAsync((Inventory)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedInventory);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedInventory = new Inventory { ItemId = "" }; // Invalid (empty ItemId)
            _controller.ModelState.AddModelError("ItemId", "The ItemId field is required.");

            var result = await _controller.Update(5, updatedInventory);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingInventory_ReturnsNoContent()
        {
            _mockInventoryService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Inventory", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Inventory 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingInventory_ReturnsNotFound()
        {
            _mockInventoryService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
