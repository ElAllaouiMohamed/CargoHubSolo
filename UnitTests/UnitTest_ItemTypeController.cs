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
    public class UnitTest_ItemTypeController
    {
        private Mock<IItemTypeService>? _mockItemTypeService;
        private Mock<ILoggingService>? _mockLoggingService;
        private ItemTypeController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockItemTypeService = new Mock<IItemTypeService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new ItemTypeController(_mockItemTypeService.Object, _mockLoggingService.Object);


            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithItemTypes()
        {
            var itemTypes = new List<Item_Type> { new Item_Type { Id = 1, Name = "Type A" } };
            _mockItemTypeService!.Setup(s => s.GetItemTypesAsync(1)).ReturnsAsync(itemTypes);

            var result = await _controller!.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemTypes, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllItemTypes()
        {
            var itemTypes = new List<Item_Type> { new Item_Type { Id = 2, Name = "Type B" } };
            _mockItemTypeService!.Setup(s => s.GetAllItemTypesAsync()).ReturnsAsync(itemTypes);

            var result = await _controller!.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemTypes, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithItemType()
        {
            var itemType = new Item_Type { Id = 3, Name = "Type C" };
            _mockItemTypeService!.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(itemType);

            var result = await _controller!.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemType, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockItemTypeService!.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Item_Type?)null);

            var result = await _controller!.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidItemType_ReturnsCreatedAtAction()
        {
            var itemType = new Item_Type { Name = "NewType" };
            var createdItemType = new Item_Type { Id = 4, Name = "NewType" };
            _mockItemTypeService!.Setup(s => s.AddItemTypeAsync(itemType)).ReturnsAsync(createdItemType);
            _controller!.ModelState.Clear();

            var result = await _controller.Create(itemType);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdItemType, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues!["id"]);
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemType", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemType created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var itemType = new Item_Type { Name = "" }; // Invalid (missing required fields)
            _controller!.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.Create(itemType);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingItemType_ReturnsOk()
        {
            var updatedItemType = new Item_Type { Name = "UpdatedType" };
            var resultItemType = new Item_Type { Id = 5, Name = "UpdatedType" };
            _mockItemTypeService!.Setup(s => s.UpdateItemTypeAsync(5, updatedItemType)).ReturnsAsync(resultItemType);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(5, updatedItemType);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultItemType, okResult.Value);
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemType", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemType 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingItemType_ReturnsNotFound()
        {
            var updatedItemType = new Item_Type { Name = "NonexistentType" };
            _mockItemTypeService!.Setup(s => s.UpdateItemTypeAsync(999, updatedItemType)).ReturnsAsync((Item_Type?)null);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(999, updatedItemType);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedItemType = new Item_Type { Name = "" }; // Invalid (missing required fields)
            _controller!.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.Update(5, updatedItemType);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingItemType_ReturnsNoContent()
        {
            _mockItemTypeService!.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller!.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemType", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemType 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingItemType_ReturnsNotFound()
        {
            _mockItemTypeService!.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller!.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
