using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CargohubV2.Controllers;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Http;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_ClientsController
    {
        private Mock<IClientService> _mockClientService;
        private Mock<ILoggingService> _mockLoggingService;
        private ClientsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockClientService = new Mock<IClientService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new ClientsController(_mockClientService.Object, _mockLoggingService.Object);

            // HttpContext mocken zodat logging geen nullref veroorzaakt
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithClients()
        {
            var clients = new List<Client> { new Client { Id = 1, Name = "Client A" } };
            _mockClientService.Setup(s => s.GetClientsAsync(1)).ReturnsAsync(clients);

            var result = await _controller.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(clients, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllClients()
        {
            var clients = new List<Client> { new Client { Id = 2, Name = "Client B" } };
            _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(clients);

            var result = await _controller.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(clients, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithClient()
        {
            var client = new Client { Id = 3, Name = "Client C" };
            _mockClientService.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(client);

            var result = await _controller.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(client, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockClientService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Client?)null);

            var result = await _controller.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidClient_ReturnsCreatedAtAction()
        {
            var client = new Client { Id = 4, Name = "New Client" };
            _mockClientService.Setup(s => s.AddClientAsync(client)).ReturnsAsync(client);

            var result = await _controller.Create(client);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.AreEqual(client, createdResult.Value);

            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Client", "Create", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task Update_ExistingClient_ReturnsOk()
        {
            var updatedClient = new Client { Id = 5, Name = "Updated Client" };
            _mockClientService.Setup(s => s.UpdateClientAsync(5, updatedClient)).ReturnsAsync(updatedClient);

            var result = await _controller.Update(5, updatedClient);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.AreEqual(updatedClient, okResult.Value);

            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Client", "Update", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task Update_NonExistingClient_ReturnsNotFound()
        {
            var updatedClient = new Client { Id = 999, Name = "Nonexistent Client" };
            _mockClientService.Setup(s => s.UpdateClientAsync(999, updatedClient)).ReturnsAsync((Client?)null);

            var result = await _controller.Update(999, updatedClient);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));

            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Client", "Update", It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task SoftDelete_ExistingClient_ReturnsNoContent()
        {
            _mockClientService.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Client", "Delete", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingClient_ReturnsNotFound()
        {
            _mockClientService.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

            _mockLoggingService.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Client", "Delete", It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
