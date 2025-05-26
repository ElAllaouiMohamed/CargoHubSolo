using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CargohubV2.Models;
using CargohubV2.Contexts;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_LocationService
    {
        private CargoHubDbContext _dbContext;
        private Mock<ILoggingService> _mockLogging;
        private LocationService _locationService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLogging = new Mock<ILoggingService>();
            _locationService = new LocationService(_dbContext, _mockLogging.Object);
        }

        [TestMethod]
        public async Task AddLocationAsync_ShouldAddLocationAndLog()
        {
            var location = new Location
            {
                WarehouseId = 1,
                Code = "LOC001",
                Name = "Test Location"
            };

            var result = await _locationService.AddLocationAsync(location);

            Assert.IsNotNull(result);
            Assert.AreEqual("LOC001", result.Code);
            Assert.IsTrue(result.Id != 0);

            _mockLogging.Verify(m => m.LogAsync("system", "Location", "Create", "/api/v1/locations", It.IsAny<string>()), Times.Once);

            var dbLocation = await _dbContext.Locations.FindAsync(result.Id);
            Assert.IsNotNull(dbLocation);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnLocation_WhenExists()
        {
            var location = new Location { Code = "LOC002", WarehouseId = 2 };
            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync();

            var result = await _locationService.GetByIdAsync(location.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("LOC002", result.Code);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _locationService.GetByIdAsync(-1);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateLocationAsync_ShouldUpdateAndLog_WhenExists()
        {
            var location = new Location { Code = "LOC003", Name = "Old Name", WarehouseId = 3 };
            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync();

            var updated = new Location { Code = "LOC003-updated", Name = "New Name", WarehouseId = 3 };

            var result = await _locationService.UpdateLocationAsync(location.Id, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual("LOC003-updated", result.Code);
            Assert.AreEqual("New Name", result.Name);

            _mockLogging.Verify(m => m.LogAsync("system", "Location", "Update", $"/api/v1/locations/{location.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateLocationAsync_ShouldReturnNull_WhenNotExists()
        {
            var updated = new Location { Code = "NoCode" };
            var result = await _locationService.UpdateLocationAsync(-1, updated);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldSoftDeleteAndLog_WhenExists()
        {
            var location = new Location { Code = "LOC004" };
            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync();

            var result = await _locationService.SoftDeleteByIdAsync(location.Id);

            Assert.IsTrue(result);

            var deletedLocation = await _dbContext.Locations.FindAsync(location.Id);
            Assert.IsTrue(deletedLocation.IsDeleted);

            _mockLogging.Verify(m => m.LogAsync("system", "Location", "Delete", $"/api/v1/locations/{location.Id}", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ShouldReturnFalse_WhenNotExists()
        {
            var result = await _locationService.SoftDeleteByIdAsync(-1);
            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }
    }
}
