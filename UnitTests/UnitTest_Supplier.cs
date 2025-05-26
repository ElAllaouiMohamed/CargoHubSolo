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
    public class SupplierServiceTests
    {
        private CargoHubDbContext? _dbContext;
        private SupplierService? _supplierService;
        private Mock<ILoggingService>? _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDatabase_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<ILoggingService>();
            _supplierService = new SupplierService(_dbContext, _mockLoggingService.Object);
            SeedDatabase(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }

        private void SeedDatabase(CargoHubDbContext context)
        {
            context.Suppliers.AddRange(
                new Supplier
                {
                    Id = 1,
                    Code = "SUP001",
                    Name = "Supplier A",
                    Address = "123 Main St",
                    AddressExtra = "Unit 1",
                    City = "Amsterdam",
                    ZipCode = "1012 AB",
                    Province = "Noord-Holland",
                    Country = "Netherlands",
                    ContactName = "John Doe",
                    PhoneNumber = "+31201234567",
                    Reference = "REF001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Supplier
                {
                    Id = 2,
                    Code = "SUP002",
                    Name = "Supplier B",
                    Address = "456 High St",
                    AddressExtra = "Unit 2",
                    City = "Brussels",
                    ZipCode = "1000",
                    Province = "Brussels",
                    Country = "Belgium",
                    ContactName = "Jane Smith",
                    PhoneNumber = "+3229876543",
                    Reference = "REF002",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Supplier
                {
                    Id = 3,
                    Code = "SUP003",
                    Name = "Supplier C",
                    Address = "789 Broad St",
                    AddressExtra = "Unit 3",
                    City = "Berlin",
                    ZipCode = "10115",
                    Province = "Berlin",
                    Country = "Germany",
                    ContactName = "Max Mustermann",
                    PhoneNumber = "+49301234567",
                    Reference = "REF003",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = true
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetSuppliersAsync_ShouldReturnLimitedNonDeletedSuppliers()
        {
            var suppliers = await _supplierService!.GetSuppliersAsync(2);
            Assert.AreEqual(2, suppliers.Count);
            Assert.IsTrue(suppliers.All(s => !s.IsDeleted));
            Assert.AreEqual("Supplier A", suppliers[0].Name);
            Assert.AreEqual("Supplier B", suppliers[1].Name);
        }

        [TestMethod]
        public async Task GetAllSuppliersAsync_ShouldReturnAllNonDeletedSuppliers()
        {
            var suppliers = await _supplierService!.GetAllSuppliersAsync();
            Assert.AreEqual(2, suppliers.Count);
            Assert.IsTrue(suppliers.All(s => !s.IsDeleted));
            Assert.IsTrue(suppliers.Any(s => s.Name == "Supplier A"));
            Assert.IsTrue(suppliers.Any(s => s.Name == "Supplier B"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnSupplier()
        {
            var supplier = await _supplierService!.GetByIdAsync(1);
            Assert.IsNotNull(supplier);
            Assert.AreEqual("Supplier A", supplier.Name);
            Assert.IsFalse(supplier.IsDeleted);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var supplier = await _supplierService!.GetByIdAsync(999);
            Assert.IsNull(supplier);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeletedId_ShouldReturnNull()
        {
            var supplier = await _supplierService!.GetByIdAsync(3);
            Assert.IsNull(supplier);
        }

        [TestMethod]
        public async Task AddSupplierAsync_ValidSupplier_ShouldAddAndLog()
        {
            var newSupplier = new Supplier
            {
                Code = "SUP004",
                Name = "Supplier D",
                Address = "101 Park Ave",
                AddressExtra = "Unit 4",
                City = "Paris",
                ZipCode = "75001",
                Province = "Île-de-France",
                Country = "France",
                ContactName = "Alice Dupont",
                PhoneNumber = "+33123456789",
                Reference = "REF004"
            };

            var addedSupplier = await _supplierService!.AddSupplierAsync(newSupplier);
            Assert.IsNotNull(addedSupplier);
            Assert.AreEqual("Supplier D", addedSupplier.Name);
            Assert.IsFalse(addedSupplier.IsDeleted);
            Assert.IsTrue(addedSupplier.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(addedSupplier.UpdatedAt > DateTime.MinValue);

            var dbSupplier = await _dbContext!.Suppliers.FindAsync(addedSupplier.Id);
            Assert.IsNotNull(dbSupplier);
            Assert.AreEqual("Supplier D", dbSupplier.Name);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Supplier", "Create", "/api/v1/suppliers", $"Created supplier {addedSupplier.Id}"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateSupplierAsync_ExistingId_ShouldUpdateAndLog()
        {
            var updatedSupplier = new Supplier
            {
                Code = "SUP001-Updated",
                Name = "Supplier A-Updated",
                Address = "123 Updated St",
                AddressExtra = "Unit 1-Updated",
                City = "Rotterdam",
                ZipCode = "3012 CD",
                Province = "Zuid-Holland",
                Country = "Netherlands",
                ContactName = "Bob Johnson",
                PhoneNumber = "+31109876543",
                Reference = "REF001-Updated"
            };

            var result = await _supplierService!.UpdateSupplierAsync(1, updatedSupplier);
            Assert.IsNotNull(result);
            Assert.AreEqual("Supplier A-Updated", result.Name);
            Assert.AreEqual("SUP001-Updated", result.Code);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);

            var dbSupplier = await _dbContext!.Suppliers.FindAsync(1);
            Assert.IsNotNull(dbSupplier);
            Assert.AreEqual("Supplier A-Updated", dbSupplier.Name);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Supplier", "Update", "/api/v1/suppliers/1", "Updated supplier 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task UpdateSupplierAsync_NonExistingId_ShouldReturnNull()
        {
            var updatedSupplier = new Supplier
            {
                Code = "SUP999",
                Name = "Supplier X",
                Address = "999 Unknown St",
                City = "Unknown",
                ZipCode = "0000",
                Country = "Unknown",
                ContactName = "Unknown",
                PhoneNumber = "+9999999999",
                Reference = "REF999"
            };

            var result = await _supplierService!.UpdateSupplierAsync(999, updatedSupplier);
            Assert.IsNull(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            var result = await _supplierService!.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);
            var supplier = await _dbContext!.Suppliers.FindAsync(1);
            Assert.IsNotNull(supplier);
            Assert.IsTrue(supplier.IsDeleted);
            Assert.IsTrue(supplier.UpdatedAt > supplier.CreatedAt);

            _mockLoggingService!.Verify(
                ls => ls.LogAsync("system", "Supplier", "Delete", "/api/v1/suppliers/1", "Soft deleted supplier 1"),
                Times.Once());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _supplierService!.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_AlreadyDeletedId_ShouldReturnFalse()
        {
            var result = await _supplierService!.SoftDeleteByIdAsync(3);
            Assert.IsFalse(result);
            _mockLoggingService!.Verify(
                ls => ls.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }
    }
}
