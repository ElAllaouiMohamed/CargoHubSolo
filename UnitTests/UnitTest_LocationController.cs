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
    public class UnitTest_LocationController
    {
        private Mock<ILocationService>? _mockLocationService;
        private Mock<ILoggingService>? _mockLoggingService;
        private LocationController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationService = new Mock<ILocationService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new LocationController(_mockLocationService.Object, _mockLoggingService.Object);


            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAll_WithLimit_ReturnsOkWithLocations()
        {
            var locations = new List<Location> { new Location { Id = 1, Name = "Loc A" } };
            _mockLocationService!.Setup(s => s.GetLocationsAsync(1)).ReturnsAsync(locations);

            var result = await _controller!.GetAll(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.IsNotNull(result);
            Assert.AreEqual(locations, okResult.Value);
        }

        [TestMethod]
        public async Task GetAll_WithoutLimit_ReturnsOkWithAllLocations()
        {
            var locations = new List<Location> { new Location { Id = 2, Name = "Loc B" } };
            _mockLocationService!.Setup(s => s.GetAllLocationsAsync()).ReturnsAsync(locations);

            var result = await _controller!.GetAll(null);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.IsNotNull(result);
            Assert.AreEqual(locations, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsOkWithLocation()
        {
            var location = new Location { Id = 3, Name = "Loc C" };
            _mockLocationService!.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(location);

            var result = await _controller!.GetById(3);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.IsNotNull(result);
            Assert.AreEqual(location, okResult.Value);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            _mockLocationService!.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Location?)null);

            var result = await _controller!.GetById(999);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ValidLocation_ReturnsCreatedAtAction()
        {
            var location = new Location { Name = "New Loc" };
            var createdLocation = new Location { Id = 4, Name = "New Loc" };
            _mockLocationService!.Setup(s => s.AddLocationAsync(location)).ReturnsAsync(createdLocation);
            _controller!.ModelState.Clear();

            var result = await _controller.Create(location);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result!;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(createdLocation, createdResult.Value);
            Assert.AreEqual("GetById", createdResult.ActionName);
            Assert.AreEqual(4, createdResult.RouteValues["id"]);
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Location", "Create", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Location created with Id 4"))), Times.Once());
        }

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var location = new Location { Name = "" }; // Invalid (missing required fields)
            _controller!.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.Create(location);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Update_ExistingLocation_ReturnsOk()
        {
            var updatedLocation = new Location { Name = "Updated Loc" };
            var resultLocation = new Location { Id = 5, Name = "Updated Loc" };
            _mockLocationService!.Setup(s => s.UpdateLocationAsync(5, updatedLocation)).ReturnsAsync(resultLocation);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(5, updatedLocation);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            Assert.IsNotNull(result);
            Assert.AreEqual(resultLocation, okResult.Value);
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Location", "Update", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Location 5 updated"))), Times.Once());
        }

        [TestMethod]
        public async Task Update_NonExistingLocation_ReturnsNotFound()
        {
            var updatedLocation = new Location { Name = "Nonexistent Loc" };
            _mockLocationService!.Setup(s => s.UpdateLocationAsync(999, updatedLocation)).ReturnsAsync((Location?)null);
            _controller!.ModelState.Clear();

            var result = await _controller.Update(999, updatedLocation);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task SoftDelete_ExistingLocation_ReturnsNoContent()
        {
            _mockLocationService!.Setup(s => s.SoftDeleteByIdAsync(6)).ReturnsAsync(true);

            var result = await _controller!.SoftDelete(6);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), "Location", "Delete", It.IsAny<string>(), It.Is<string>(msg => msg.Contains("Location 6 soft-deleted"))), Times.Once());
        }

        [TestMethod]
        public async Task SoftDelete_NonExistingLocation_ReturnsNotFound()
        {
            _mockLocationService!.Setup(s => s.SoftDeleteByIdAsync(999)).ReturnsAsync(false);

            var result = await _controller!.SoftDelete(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockLoggingService!.Verify(l => l.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
