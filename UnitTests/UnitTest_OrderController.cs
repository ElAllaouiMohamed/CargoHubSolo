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
    public class UnitTest_OrdersController
    {
        private Mock<IOrderService> _mockOrderService;
        private Mock<ILoggingService> _mockLoggingService;
        private OrdersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new OrdersController(_mockOrderService.Object, _mockLoggingService.Object);

            // HttpContext mocken zodat logging geen nullref veroorzaakt
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithOrders()
        {
            var orders = new List<Order> { new Order { Id = 1, Reference = "ORD001" } };
            _mockOrderService.Setup(s => s.GetOrdersAsync(1)).ReturnsAsync(orders);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(orders, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllOrders()
        {
            var orders = new List<Order> { new Order { Id = 2, Reference = "ORD002" } };
            _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(orders, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithOrder()
        {
            var order = new Order { Id = 3, Reference = "ORD003" };
            _mockOrderService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(order);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(order, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Order)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidOrder_ReturnsCreatedAtAction()
        {
            var order = new Order { Reference = "NewOrder", SourceId = 1 };
            var createdOrder = new Order { Id = 4, Reference = "NewOrder", SourceId = 1 };
            _mockOrderService.Setup(s => s.AddOrderAsync(order)).ReturnsAsync(createdOrder);
            _controller.ModelState.Clear();

            var result = await _controller.Create(order);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdOrder, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Order", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Order created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var order = new Order { Reference = "" }; // Invalid (missing required fields)
            _controller.ModelState.AddModelError("SourceId", "The SourceId field is required.");

            var result = await _controller.Create(order);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingOrder_ReturnsOk()
        {
            var updatedOrder = new Order { Reference = "UpdatedOrder", SourceId = 1 };
            var resultOrder = new Order { Id = 5, Reference = "UpdatedOrder", SourceId = 1 };
            _mockOrderService.Setup(s => s.UpdateOrderAsync(5, updatedOrder)).ReturnsAsync(resultOrder);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedOrder);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultOrder, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Order", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Order 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingOrder_ReturnsNotFound()
        {
            var updatedOrder = new Order { Reference = "NonexistentOrder", SourceId = 1 };
            _mockOrderService.Setup(s => s.UpdateOrderAsync(999, updatedOrder)).ReturnsAsync((Order)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedOrder);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedOrder = new Order { Reference = "" }; // Invalid (missing required fields)
            _controller.ModelState.AddModelError("SourceId", "The SourceId field is required.");

            var result = await _controller.Update(5, updatedOrder);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingOrder_ReturnsNoContent()
        {
            _mockOrderService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Order", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Order 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingOrder_ReturnsNotFound()
        {
            _mockOrderService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
