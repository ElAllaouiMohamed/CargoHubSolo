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
    public class UnitTest_TransfersController
    {
        private Mock<ITransferService> _mockTransferService;
        private Mock<ILoggingService> _mockLoggingService;
        private TransfersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockTransferService = new Mock<ITransferService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new TransfersController(_mockTransferService.Object, _mockLoggingService.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithTransfers()
        {
            var transfers = new List<Transfer> { new Transfer { Id = 1, Reference = "TRF001" } };
            _mockTransferService.Setup(s => s.GetTransfersAsync(1)).ReturnsAsync(transfers);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(transfers, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllTransfers()
        {
            var transfers = new List<Transfer> { new Transfer { Id = 2, Reference = "TRF002" } };
            _mockTransferService.Setup(s => s.GetAllTransfersAsync()).ReturnsAsync(transfers);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(transfers, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithTransfer()
        {
            var transfer = new Transfer { Id = 3, Reference = "TRF003" };
            _mockTransferService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(transfer);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(transfer, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockTransferService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Transfer)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidTransfer_ReturnsCreatedAtAction()
        {
            var transfer = new Transfer { Reference = "NewTransfer", TransferFrom = 1, TransferTo = 2 };
            var createdTransfer = new Transfer { Id = 4, Reference = "NewTransfer", TransferFrom = 1, TransferTo = 2 };
            _mockTransferService.Setup(s => s.AddTransferAsync(transfer)).ReturnsAsync(createdTransfer);
            _controller.ModelState.Clear();

            var result = await _controller.Create(transfer);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(createdTransfer, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Transfer", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Transfer created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var transfer = new Transfer { Reference = "" }; // Invalid (missing required fields)
            _controller.ModelState.AddModelError("TransferFrom", "The TransferFrom field is required.");

            var result = await _controller.Create(transfer);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingTransfer_ReturnsOk()
        {
            var updatedTransfer = new Transfer { Reference = "UpdatedTransfer", TransferFrom = 1, TransferTo = 2 };
            var resultTransfer = new Transfer { Id = 5, Reference = "UpdatedTransfer", TransferFrom = 1, TransferTo = 2 };
            _mockTransferService.Setup(s => s.UpdateTransferAsync(5, updatedTransfer)).ReturnsAsync(resultTransfer);
            _controller.ModelState.Clear();

            var result = await _controller.Update(5, updatedTransfer);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(resultTransfer, okResult.Value);
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Transfer", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Transfer 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingTransfer_ReturnsNotFound()
        {
            var updatedTransfer = new Transfer { Reference = "NonexistentTransfer", TransferFrom = 1, TransferTo = 2 };
            _mockTransferService.Setup(s => s.UpdateTransferAsync(999, updatedTransfer)).ReturnsAsync((Transfer)null);
            _controller.ModelState.Clear();

            var result = await _controller.Update(999, updatedTransfer);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            var updatedTransfer = new Transfer { Reference = "" }; // Invalid (missing required fields)
            _controller.ModelState.AddModelError("TransferFrom", "The TransferFrom field is required.");

            var result = await _controller.Update(5, updatedTransfer);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingTransfer_ReturnsNoContent()
        {
            _mockTransferService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Transfer", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Transfer 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingTransfer_ReturnsNotFound()
        {
            _mockTransferService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
