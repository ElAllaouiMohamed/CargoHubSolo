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
    public class UnitTest_SupplierController
    {
        private Mock<ISupplierService>? _mockSupplierService;
        private Mock<ILoggingService>? _mockLoggingService;
        private SupplierController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockSupplierService = new Mock<ISupplierService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new SupplierController(_mockSupplierService.Object, _mockLoggingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithSuppliers()
        {
            var suppliers = new List<Supplier> { new Supplier { Id = 1, Name = "Supplier A" } };
            _mockSupplierService!.Setup(s => s.GetSuppliersAsync(1)).ReturnsAsync(suppliers);

            var result = await _controller!.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(suppliers, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllSuppliers()
        {
            var suppliers = new List<Supplier> { new Supplier { Id = 2, Name = "Supplier B" } };
            _mockSupplierService!.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(suppliers);

            var result = await _controller!.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(suppliers, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithSupplier()
        {
            var supplier = new Supplier { Id = 3, Name = "Supplier C" };
            _mockSupplierService!.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(supplier);

            var result = await _controller!.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(supplier, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockSupplierService!.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Supplier?)null);

            var result = await _controller!.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidSupplier_ReturnsCreatedAtAction()
        {
            var supplier = new Supplier { Name = "New Supplier" };
            var createdSupplier = new Supplier { Id = 4, Name = "New Supplier" };
            _mockSupplierService!.Setup(s => s.AddSupplierAsync(supplier)).ReturnsAsync(createdSupplier);
            _controller!.ModelState.Clear();

            var result = await _controller.Create(supplier);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdSupplier, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);

            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Supplier", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Supplier created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_ExistingSupplier_ReturnsOk()
        {
            var updatedSupplier = new Supplier { Name = "Updated Supplier" };
            var resultSupplier = new Supplier { Id = 5, Name = "Updated Supplier" };
            _mockSupplierService!.Setup(s => s.UpdateSupplierAsync(5, updatedSupplier)).ReturnsAsync(resultSupplier);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(5, updatedSupplier);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultSupplier, okResult.Value);

            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Supplier", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Supplier 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingSupplier_ReturnsNotFound()
        {
            var updatedSupplier = new Supplier { Name = "Nonexistent Supplier" };
            _mockSupplierService!.Setup(s => s.UpdateSupplierAsync(999, updatedSupplier)).ReturnsAsync((Supplier?)null);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(999, updatedSupplier);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));

            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Supplier", "Update", It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingSupplier_ReturnsNoContent()
        {
            _mockSupplierService!.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller!.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Supplier", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Supplier 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingSupplier_ReturnsNotFound()
        {
            _mockSupplierService!.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller!.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Supplier", "Delete", It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
