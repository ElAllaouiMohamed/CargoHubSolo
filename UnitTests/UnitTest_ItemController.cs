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
    public class UnitTest_ItemsController
    {
        private Mock<IItemService> _mockItemService;
        private Mock<ILoggingService> _mockLoggingService;
        private ItemsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockItemService = new Mock<IItemService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new ItemsController(_mockItemService.Object, _mockLoggingService.Object);


            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithItems()
        {
            var items = new List<Item> { new Item { Id = 1, Code = "ITEM001" } };
            _mockItemService.Setup(s => s.GetItemsAsync(1)).ReturnsAsync(items);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(items, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllItems()
        {
            var items = new List<Item> { new Item { Id = 2, Code = "ITEM002" } };
            _mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(items);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(items, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithItem()
        {
            var item = new Item { Id = 3, Code = "ITEM003" };
            _mockItemService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(item);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(item, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockItemService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Item)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidItem_ReturnsCreatedAtAction()
        {
            var item = new Item { Code = "NewItem", Description = "Test" };
            var createdItem = new Item { Id = 4, Code = "NewItem", Description = "Test" };
            _mockItemService.Setup(s => s.AddItemAsync(item)).ReturnsAsync(createdItem);
            _controller.ModelState.Clear();

            var result = await _controller.Create(item);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdItem, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Item", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Item created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var item = new Item { Code = "" }; // Invalid (empty Code)
            _controller.ModelState.AddModelError("Code", "The Code field is required.");

            var result = await _controller.Create(item);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingItem_ReturnsOk()
        {
            var updatedItem = new Item { Code = "UpdatedItem", Description = "Updated" };
            var resultItem = new Item { Id = 5, Code = "UpdatedItem", Description = "Updated" };
            _mockItemService.Setup(s => s.UpdateItemAsync(5, updatedItem)).ReturnsAsync(resultItem);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedItem);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultItem, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Item", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Item 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingItem_ReturnsNotFound()
        {
            var updatedItem = new Item { Code = "NonexistentItem", Description = "Test" };
            _mockItemService.Setup(s => s.UpdateItemAsync(999, updatedItem)).ReturnsAsync((Item)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedItem);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedItem = new Item { Code = "" }; // Invalid (empty Code)
            _controller.ModelState.AddModelError("Code", "The Code field is required.");

            var result = await _controller.Update(5, updatedItem);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingItem_ReturnsNoContent()
        {
            _mockItemService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Item", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Item 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingItem_ReturnsNotFound()
        {
            _mockItemService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
