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
    public class UnitTest_ItemGroupController
    {
        private Mock<IItemGroupService> _mockItemGroupService;
        private Mock<ILoggingService> _mockLoggingService;
        private ItemGroupController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockItemGroupService = new Mock<IItemGroupService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new ItemGroupController(_mockItemGroupService.Object, _mockLoggingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithItemGroups()
        {
            var itemGroups = new List<Item_Group> { new Item_Group { Id = 1, Name = "Group A" } };
            _mockItemGroupService.Setup(s => s.GetItemGroupsAsync(1)).ReturnsAsync(itemGroups);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemGroups, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllItemGroups()
        {
            var itemGroups = new List<Item_Group> { new Item_Group { Id = 2, Name = "Group B" } };
            _mockItemGroupService.Setup(s => s.GetAllItemGroupsAsync()).ReturnsAsync(itemGroups);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemGroups, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithItemGroup()
        {
            var itemGroup = new Item_Group { Id = 3, Name = "Group C" };
            _mockItemGroupService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(itemGroup);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(itemGroup, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockItemGroupService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Item_Group)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidItemGroup_ReturnsCreatedAtAction()
        {
            var itemGroup = new Item_Group { Name = "New Group", Description = "Test" };
            var createdItemGroup = new Item_Group { Id = 4, Name = "New Group", Description = "Test" };
            _mockItemGroupService.Setup(s => s.AddItemGroupAsync(itemGroup)).ReturnsAsync(createdItemGroup);
            _controller.ModelState.Clear();

            var result = await _controller.Create(itemGroup);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdItemGroup, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemGroup", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemGroup created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var itemGroup = new Item_Group { Name = "123" }; // Invalid name (contains numbers)
            _controller.ModelState.AddModelError("Name", "Numbers and special characters are not allowed");

            var result = await _controller.Create(itemGroup);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingItemGroup_ReturnsOk()
        {
            var updatedItemGroup = new Item_Group { Name = "Updated Group", Description = "Updated" };
            var resultItemGroup = new Item_Group { Id = 5, Name = "Updated Group", Description = "Updated" };
            _mockItemGroupService.Setup(s => s.UpdateItemGroupAsync(5, updatedItemGroup)).ReturnsAsync(resultItemGroup);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedItemGroup);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultItemGroup, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemGroup", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemGroup 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingItemGroup_ReturnsNotFound()
        {
            var updatedItemGroup = new Item_Group { Name = "Nonexistent Group", Description = "Test" };
            _mockItemGroupService.Setup(s => s.UpdateItemGroupAsync(999, updatedItemGroup)).ReturnsAsync((Item_Group)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedItemGroup);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedItemGroup = new Item_Group { Name = "123" }; // Invalid name
            _controller.ModelState.AddModelError("Name", "Numbers and special characters are not allowed");

            var result = await _controller.Update(5, updatedItemGroup);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingItemGroup_ReturnsNoContent()
        {
            _mockItemGroupService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "ItemGroup", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("ItemGroup 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingItemGroup_ReturnsNotFound()
        {
            _mockItemGroupService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
