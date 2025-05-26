using Microsoft.EntityFrameworkCore;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace UnitTests
{
    [TestClass]
    public class UnitTest_ClientService
    {
        private CargoHubDbContext _dbContext;
        private ClientService _clientService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCargoHubDatabase")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _clientService = new ClientService(_dbContext);
            SeedDatabase(_dbContext);
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Clients.AddRange(
                new Client
                {
                    ClientId = 1,
                    Name = "boss",
                    Address = "123 Street A",
                    City = "Roosendaal",
                    ZipCode = "12345",
                    Province = "Brabant",
                    Country = "Netherlands",
                    ContactName = "Contact A",
                    ContactPhone = "1234567890",
                    ContactEmail = "hank@gmail.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Client
                {
                    ClientId = 2,
                    Name = "boss",
                    Address = "456 Street B",
                    City = "City1",
                    ZipCode = "67890",
                    Province = "Kiev",
                    Country = "Ukraine",
                    ContactName = "timmi",
                    ContactPhone = "011231231",
                    ContactEmail = "timmi@gmail.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetClientsAsync_ShouldReturnAllClients()
        {
            var clients = await _clientService.GetClientsAsync();
            Assert.AreEqual(2, clients.Count());
        }

        [TestMethod]
        public async Task GetClientAsync_ExistingId_ShouldReturnClient()
        {
            var client = await _clientService.GetClientAsync(1);
            Assert.IsNotNull(client);
            Assert.AreEqual("Vincent", client.Name);
        }

        [TestMethod]
        public async Task GetClientAsync_NonExistingId_ShouldReturnNull()
        {
            var client = await _clientService.GetClientAsync(999);
            Assert.IsNull(client);
        }

        [TestMethod]
        public async Task AddClientAsync_ShouldAddClient()
        {
            var newClient = await _clientService.AddClientAsync(
                "New Client",
                "789 Street C",
                "CityC",
                "33333",
                "ProvinceC",
                "CountryC",
                "Contact C",
                "1231231234",
                "contactc@example.com");

            Assert.IsNotNull(newClient);
            var allClients = await _clientService.GetClientsAsync();
            Assert.AreEqual(3, allClients.Count());
        }

        [TestMethod]
        public async Task UpdateClientAsync_ExistingId_ShouldUpdateClient()
        {
            var updatedClient = await _clientService.UpdateClientAsync(
                1,
                "Updated",
                "Updated Address",
                "Updated City",
                "00000",
                "Updated Province",
                "Updated Country",
                "Updated Contact",
                "0000000000",
                "updated@example.com");

            Assert.IsNotNull(updatedClient);
            Assert.AreEqual("Updated", updatedClient.Name);
        }

        [TestMethod]
        public async Task UpdateClientAsync_NonExistingId_ShouldThrowKeyNotFoundException()
        {
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                _clientService.UpdateClientAsync(
                    999,
                    "Name",
                    "Address",
                    "City",
                    "Zip",
                    "Province",
                    "Country",
                    "Contact",
                    "Phone",
                    "Email@example.com"));
        }

        [TestMethod]
        public async Task DeleteClientAsync_ExistingId_ShouldDeleteClient()
        {
            var result = await _clientService.DeleteClientAsync(1);
            Assert.IsTrue(result);

            var client = await _clientService.GetClientAsync(1);
            Assert.IsNull(client);
        }

        [TestMethod]
        public async Task DeleteClientAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _clientService.DeleteClientAsync(999);
            Assert.IsFalse(result);
        }
    }
}
