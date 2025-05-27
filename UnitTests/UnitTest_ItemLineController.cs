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
    public class UnitTest_ItemLineController
    {
        private Mock<IItemLineService> _mockItemLineService;
        private Mock<ILoggingService> _mockLoggingService;
        private ItemLineController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockItemLineService = new Mock<IItemLineService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new ItemLineController(_mockItemLineService.Object, _mockLoggingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithItemLines()
        {
            var itemLines = new List<Item_Line> { new Item_Line { Id = 1, Name = "Line A" } };
            _mockItemLineService.Setup(s => s.GetItemLinesAsync(1)).ReturnsAsync(itemLines);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemLines, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllItemLines()
        {
            var itemLines = new List<Item_Line> { new Item_Line { Id = 2, Name = "Line B" } };
            _mockItemLineService.Setup(s => s.GetAllItemLinesAsync()).ReturnsAsync(itemLines);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemLines, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithItemLine()
        {
            var itemLine = new Item_Line { Id = 3, Name = "Line C" };
            _mockItemLineService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(itemLine);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemLine, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockItemLineService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Item_Line)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidItemLine_ReturnsCreatedAtAction()
        {
            var itemLine = new Item_Line { Name = "New Line", Description = "Test" };
            var createdItemLine = new Item_Line { Id = 4, Name = "New Line", Description = "Test" };
            _mockItemLineService.Setup(s => s.AddItemLineAsync(itemLine)).ReturnsAsync(createdItemLine);
            _controller.ModelState.Clear();

            var result = await _controller.Create(itemLine);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdItemLine, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemLine", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemLine created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var itemLine = new Item_Line { Name = "" }; // Invalid (empty name)
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.Create(itemLine);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingItemLine_ReturnsOk()
        {
            var updatedItemLine = new Item_Line { Name = "Updated Line", Description = "Updated" };
            var resultItemLine = new Item_Line { Id = 5, Name = "Updated Line", Description = "Updated" };
            _mockItemLineService.Setup(s => s.UpdateItemLineAsync(5, updatedItemLine)).ReturnsAsync(resultItemLine);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedItemLine);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultItemLine, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemLine", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemLine 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingItemLine_ReturnsNotFound()
        {
            var updatedItemLine = new Item_Line { Name = "Nonexistent Line", Description = "Test" };
            _mockItemLineService.Setup(s => s.UpdateItemLineAsync(999, updatedItemLine)).ReturnsAsync((Item_Line)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedItemLine);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedItemLine = new Item_Line { Name = "" }; // Invalid (empty name)
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.Update(5, updatedItemLine);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingItemLine_ReturnsNoContent()
        {
            _mockItemLineService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemLine", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemLine 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingItemLine_ReturnsNotFound()
        {
            _mockItemLineService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
