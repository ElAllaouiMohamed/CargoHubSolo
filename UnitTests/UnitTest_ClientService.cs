using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class UnitTest_ClientService
    {
        private CargoHubDbContext _dbContext;
        private ClientService _clientService;
        private Mock<LoggingService> _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            // Configureer in-memory database met unieke naam
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<LoggingService>();
            _clientService = new ClientService(_dbContext, _mockLoggingService.Object);
            SeedDatabase(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Clients.AddRange(
                new Client
                {
                    Id = 1,
                    Name = "Client A",
                    Address = "123 Street A",
                    City = "Amsterdam",
                    ZipCode = "1012 AB",
                    Province = "Noord-Holland",
                    Country = "Netherlands",
                    ContactName = "John Doe",
                    ContactPhone = "+31201234567",
                    ContactEmail = "john.doe@example.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Client
                {
                    Id = 2,
                    Name = "Client B",
                    Address = "456 Avenue B",
                    City = "Brussels",
                    ZipCode = "1000",
                    Province = "Brussels",
                    Country = "Belgium",
                    ContactName = "Jane Smith",
                    ContactPhone = "+3229876543",
                    ContactEmail = "jane.smith@example.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Client
                {
                    Id = 3,
                    Name = "Client C",
                    Address = "789 Road C",
                    City = "Berlin",
                    ZipCode = "10115",
                    Province = "Berlin",
                    Country = "Germany",
                    ContactName = "Max Mustermann",
                    ContactPhone = "+49301234567",
                    ContactEmail = "max.mustermann@example.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true // Soft-deleted
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetClientsAsync_ShouldReturnLimitedNonDeletedClients()
        {
            // Act
            var clients = await _clientService.GetClientsAsync(2);

            // Assert
            Assert.AreEqual(2, clients.Count);
            Assert.IsTrue(clients.All(c => !c.IsDeleted));
            Assert.AreEqual("Client A", clients[0].Name);
            Assert.AreEqual("Client B", clients[1].Name);
        }

        [TestMethod]
        public async Task GetAllClientsAsync_ShouldReturnAllNonDeletedClients()
        {
            // Act
            var clients = await _clientService.GetAllClientsAsync();

            // Assert
            Assert.AreEqual(2, clients.Count);
            Assert.IsTrue(clients.All(c => !c.IsDeleted));
            Assert.IsTrue(clients.Any(c => c.Name == "Client A"));
            Assert.IsTrue(clients.Any(c => c.Name == "Client B"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnClient()
        {
            // Act
            var client = await _clientService.GetByIdAsync(1);

            // Assert
            Assert.IsNotNull(client);
            Assert.AreEqual("Client A", client.Name);
            Assert.IsFalse(client.IsDeleted);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Act
            var client = await _clientService.GetByIdAsync(999);

            // Assert
            Assert.IsNull(client);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            // Act
            var client = await _clientService.GetByIdAsync(3);

            // Assert
            Assert.IsNull(client);
        }

        [TestMethod]
        public async Task AddClientAsync_ValidClient_ShouldAddAndLog()
        {
            // Arrange
            var newClient = new Client
            {
                Name = "New Client",
                Address = "789 Street D",
                City = "Paris",
                ZipCode = "75001",
                Province = "Île-de-France",
                Country = "France",
                ContactName = "Alice Dupont",
                ContactPhone = "+33123456789",
                ContactEmail = "alice.dupont@example.com"
            };

            // Act
            var addedClient = await _clientService.AddClientAsync(newClient);

            // Assert
            Assert.IsNotNull(addedClient);
            Assert.AreEqual("New Client", addedClient.Name);
            Assert.IsFalse(addedClient.IsDeleted);
            Assert.IsTrue(addedClient.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedClient.UpdatedAt > DateTime.MinValue);

            var dbClient = await _dbContext.Clients.FindAsync(addedClient.Id);
            Assert.IsNotNull(dbClient);
            Assert.AreEqual("New Client", dbClient.Name);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Client", "Create", "/api/v1/clients", $"Created client {addedClient.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task AddClientAsync_InvalidContactPhone_ShouldStillAdd()
        {
            // Arrange
            var newClient = new Client
            {
                Name = "New Client",
                Address = "789 Street D",
                City = "Paris",
                ZipCode = "75001",
                Province = "Île-de-France",
                Country = "France",
                ContactName = "Alice Dupont",
                ContactPhone = "invalid-phone", // Ongeldig, maar geen validatie in service
                ContactEmail = "alice.dupont@example.com"
            };

            // Act
            var addedClient = await _clientService.AddClientAsync(newClient);

            // Assert
            Assert.IsNotNull(addedClient);
            Assert.AreEqual("invalid-phone", addedClient.ContactPhone); // Service valideert niet
        }

        [TestMethod]
        public async Task UpdateClientAsync_ExistingId_ShouldUpdateAndLog()
        {
            // Arrange
            var updatedClient = new Client
            {
                Name = "Updated Client",
                Address = "Updated Address",
                City = "Rotterdam",
                ZipCode = "3012 CD",
                Province = "Zuid-Holland",
                Country = "Netherlands",
                ContactName = "Bob Johnson",
                ContactPhone = "+31109876543",
                ContactEmail = "bob.johnson@example.com"
            };

            // Act
            var result = await _clientService.UpdateClientAsync(1, updatedClient);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Client", result.Name);
            Assert.AreEqual("Updated Address", result.Address);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);

            var dbClient = await _dbContext.Clients.FindAsync(1);
            Assert.IsNotNull(dbClient);
            Assert.AreEqual("Updated Client", dbClient.Name);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Client", "Update", "/api/v1/clients/1", "Updated client 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateClientAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            var updatedClient = new Client
            {
                Name = "Updated Client",
                Address = "Updated Address",
                City = "Rotterdam",
                ZipCode = "3012 CD",
                Province = "Zuid-Holland",
                Country = "Netherlands",
                ContactName = "Bob Johnson",
                ContactPhone = "+31109876543",
                ContactEmail = "bob.johnson@example.com"
            };

            // Act
            var result = await _clientService.UpdateClientAsync(999, updatedClient);

            // Assert
            Assert.IsNull(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            // Act
            var result = await _clientService.SoftDeleteByIdAsync(1);

            // Assert
            Assert.IsTrue(result);
            var client = await _dbContext.Clients.FindAsync(1);
            Assert.IsNotNull(client);
            Assert.IsTrue(client.IsDeleted);
            Assert.IsTrue(client.UpdatedAt > client.CreatedAt);

            _mockLoggingService.Verify(
                ls => ls.LogAsync("system", "Client", "Delete", "/api/v1/clients/1", "Soft deleted client 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _clientService.SoftDeleteByIdAsync(999);

            // Assert
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            // Act
            var result = await _clientService.SoftDeleteByIdAsync(3);

            // Assert
            Assert.IsFalse(result);
            _mockLoggingService.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}