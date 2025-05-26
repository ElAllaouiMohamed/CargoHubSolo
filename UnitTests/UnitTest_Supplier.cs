namespace UnitTests;
using CargoHubV2;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Models;
using CargohubV2.Services;

[TestClass]
public class UnitTest_Supplier
{
    private CargoHubDbContext _dbContext;
    private SupplierService _supplierService;
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CargoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unieke DB per test run
            .Options;

        _dbContext = new CargoHubDbContext(options);
        SeedDatabase(_dbContext);
        _supplierService = new SupplierService(_dbContext);
    }

    private void SeedDatabase(CargoHubDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.Suppliers.AddRange(
            new Supplier
            {
                SupplierId = 1,
                Code = "SUP001",
                Name = "Supplier One",
                Address = "123 Main Street",
                AddressExtra = "Building A",
                City = "Metropolis",
                ZipCode = "12345",
                Province = "Central",
                Country = "Wonderland",
                ContactName = "John Doe",
                PhoneNumber = "123-456-7890",
                Reference = "REF123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Supplier
            {
                SupplierId = 2,
                Code = "SUP002",
                Name = "Supplier Two",
                Address = "456 Side Street",
                AddressExtra = "Building B",
                City = "Gotham",
                ZipCode = "67890",
                Province = "Central",
                Country = "Neverland",
                ContactName = "Jane Smith",
                PhoneNumber = "098-765-4321",
                Reference = "REF124",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        context.SaveChanges();
    }

    [TestMethod]
    public async Task TestGetAllSuppliers()
    {
        var supplierList = await _supplierService.GetAllSuppliersAsync();
        Assert.IsTrue(supplierList.Count >= 2);
    }

    [TestMethod]
    [DataRow(1, true)]
    [DataRow(999, false)]
    public async Task TestGetSupplierById(int supplierId, bool expectedResult)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(supplierId);
        Assert.AreEqual(expectedResult, supplier != null);
    }

    [TestMethod]
    [DataRow(3, "SUP003", "Supplier Three", "123 Example Street", "Example City", "EX123", "Exampleland", "Alice Johnson", "123-123-1234", true)]
    [DataRow(4, null, "Invalid Supplier", "No Address", "No City", "NOZIP", "Noland", "Invalid Contact", "987-654-3210", false)]
    public async Task TestPostSupplier(int supplierId, string code, string name, string address, string city, string zipCode, string country, string contactName, string phoneNumber, bool expectedResult)
    {
        var supplier = new Supplier
        {
            SupplierId = supplierId,
            Code = code,
            Name = name,
            Address = address,
            City = city,
            ZipCode = zipCode,
            Country = country,
            ContactName = contactName,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Reference = ""
        };

        var returnedSupplier = await _supplierService.CreateSupplierAsync(supplier);
        Assert.AreEqual(expectedResult, returnedSupplier != null);
    }

    [TestMethod]
    [DataRow(1, "Updated Supplier", "SUP001-NEW", "789 Update Ave", "Updated City", "UP123", "Updateland", "Updated Contact", "456-456-4567", true)]
    [DataRow(999, null, null, null, null, null, null, null, null, false)]
    public async Task TestPutSupplier(int supplierId, string name, string code, string address, string city, string zipCode, string country, string contactName, string phoneNumber, bool expectedResult)
    {
        var supplier = new Supplier
        {
            SupplierId = supplierId,
            Code = code,
            Name = name,
            Address = address,
            City = city,
            ZipCode = zipCode,
            Country = country,
            ContactName = contactName,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updated = await _supplierService.UpdateSupplierAsync(supplierId, supplier);
        Assert.AreEqual(expectedResult, updated);
    }

    [TestMethod]
    [DataRow(1, true)]
    [DataRow(999, false)]
    public async Task TestDeleteSupplier(int supplierId, bool expectedResult)
    {
        var deleted = await _supplierService.DeleteSupplierAsync(supplierId);
        Assert.AreEqual(expectedResult, deleted);
    }

    [TestMethod]
    public async Task TestDeleteAllSuppliers()
    {
        var deleted = await _supplierService.DeleteAllSuppliersAsync();
        Assert.IsTrue(deleted);
    }
}
