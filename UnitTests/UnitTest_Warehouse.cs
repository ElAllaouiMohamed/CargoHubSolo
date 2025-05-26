namespace UnitTests;
using CargoHubV2.Data;
using CargoHubV2;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[TestClass]
public class UnitTest_Warehouse
{
    private CargoHubDbContext _dbContext;
    private WarehouseService _warehouseService;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CargoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CargoHubDbContext(options);
        SeedDatabase(_dbContext);
        _warehouseService = new WarehouseService(_dbContext);
    }

    private void SeedDatabase(CargoHubDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.Warehouses.Add(new Warehouse
        {
            WarehouseId = 1,
            Code = "WH001",
            Name = "Main Warehouse",
            Address = "123 Warehouse St.",
            Zip = "12345",
            City = "Sample City",
            Province = "Sample Province",
            Country = "Sample Country",
            ContactName = "John Doe",
            ContactPhone = "555-1234",
            ContactEmail = "johndoe@example.com",
            RestrictedClassificationsList = new List<string> { "1.1" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        context.Warehouses.Add(new Warehouse
        {
            WarehouseId = 2,
            Code = "WH002",
            Name = "Backup Warehouse",
            Address = "456 Warehouse Ln.",
            Zip = "67890",
            City = "Backup City",
            Province = "Backup Province",
            Country = "Backup Country",
            ContactName = "Jane Smith",
            ContactPhone = "555-5678",
            ContactEmail = "janesmith@example.com",
            RestrictedClassificationsList = new List<string> { "2.2", "3" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        context.SaveChanges();
    }

    [TestMethod]
    public async Task TestGetAllWarehouses()
    {
        var warehouses = await _warehouseService.GetAllWarehousesAsync();
        Assert.AreEqual(2, warehouses.Count);
    }

    [TestMethod]
    public async Task TestGetWarehouseById_ValidId()
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(1);
        Assert.IsNotNull(warehouse);
        Assert.AreEqual("Main Warehouse", warehouse.Name);
    }

    [TestMethod]
    public async Task TestGetWarehouseById_InvalidId()
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(999);
        Assert.IsNull(warehouse);
    }

    [TestMethod]
    public async Task TestAddWarehouse_ValidData()
    {
        var warehouseDto = new WarehouseDto
        {
            Code = "WH003",
            Name = "New Warehouse",
            Address = "789 Warehouse Blvd.",
            Zip = "54321",
            City = "New City",
            Province = "New Province",
            Country = "New Country",
            ContactName = "New Contact",
            ContactPhone = "555-0000",
            ContactEmail = "newcontact@example.com",
            RestrictedClassificationsList = new List<string> { "4.1" }
        };

        var result = await _warehouseService.AddWarehouseAsync(warehouseDto);
        Assert.AreEqual("Warehouse successfully created.", result.message);
        Assert.IsNotNull(result.warehouse);
        Assert.AreEqual(3, result.warehouse.WarehouseId);
    }

    [TestMethod]
    public async Task TestAddWarehouse_InvalidClassification()
    {
        var warehouseDto = new WarehouseDto
        {
            Code = "WH004",
            Name = "Invalid Warehouse",
            Address = "Invalid Address",
            Zip = "00000",
            City = "Invalid City",
            Province = "Invalid Province",
            Country = "Invalid Country",
            ContactName = "Invalid Contact",
            ContactPhone = "555-9999",
            ContactEmail = "invalid@example.com",
            RestrictedClassificationsList = new List<string> { "InvalidClass" }
        };

        var result = await _warehouseService.AddWarehouseAsync(warehouseDto);
        Assert.AreEqual("Invalid classification 'InvalidClass'.", result.message);
        Assert.IsNull(result.warehouse);
    }

    [TestMethod]
    public async Task TestUpdateWarehouse_ValidData()
    {
        var warehouseDto = new WarehouseDto
        {
            Code = "WH001-Updated",
            Name = "Updated Warehouse",
            Address = "Updated Address",
            Zip = "11111",
            City = "Updated City",
            Province = "Updated Province",
            Country = "Updated Country",
            ContactName = "Updated Contact",
            ContactPhone = "555-1111",
            ContactEmail = "updated@example.com",
            RestrictedClassificationsList = new List<string> { "5.1" }
        };

        var result = await _warehouseService.UpdateWarehouseAsync(1, warehouseDto);
        Assert.AreEqual("Warehouse successfully updated.", result.message);
        Assert.IsNotNull(result.ReturnedWarehouse);
        Assert.AreEqual("Updated Warehouse", result.ReturnedWarehouse.Name);
    }

    [TestMethod]
    public async Task TestUpdateWarehouse_InvalidClassification()
    {
        var warehouseDto = new WarehouseDto
        {
            Code = "WH001",
            Name = "Warehouse",
            Address = "Address",
            Zip = "00000",
            City = "City",
            Province = "Province",
            Country = "Country",
            ContactName = "Contact",
            ContactPhone = "555-1234",
            ContactEmail = "contact@example.com",
            RestrictedClassificationsList = new List<string> { "InvalidClass" }
        };

        var result = await _warehouseService.UpdateWarehouseAsync(1, warehouseDto);
        Assert.AreEqual("Invalid classification 'InvalidClass'.", result.message);
        Assert.IsNull(result.ReturnedWarehouse);
    }

    [TestMethod]
    public async Task TestDeleteWarehouse_ValidId()
    {
        var message = await _warehouseService.DeleteWarehouseAsync(1);
        Assert.AreEqual("Warehouse successfully deleted.", message);

        var warehouse = await _warehouseService.GetWarehouseByIdAsync(1);
        Assert.IsNull(warehouse);
    }

    [TestMethod]
    public async Task TestDeleteWarehouse_InvalidId()
    {
        var message = await _warehouseService.DeleteWarehouseAsync(999);
        Assert.AreEqual("Warehouse not found.", message);
    }

    [TestMethod]
    public async Task TestAddWarehouse_ReusesDeletedId()
    {
        var deleteMessage = await _warehouseService.DeleteWarehouseAsync(2);
        Assert.AreEqual("Warehouse successfully deleted.", deleteMessage);

        var warehouseDto = new WarehouseDto
        {
            Code = "WH003",
            Name = "Reused Warehouse",
            Address = "123 Reused Address",
            Zip = "54321",
            City = "Reused City",
            Province = "Reused Province",
            Country = "Reused Country",
            ContactName = "Reused Contact",
            ContactPhone = "555-3333",
            ContactEmail = "reused@example.com",
            RestrictedClassificationsList = new List<string> { "6.1" }
        };

        var result = await _warehouseService.AddWarehouseAsync(warehouseDto);
        Assert.AreEqual("Warehouse successfully created.", result.message);
        Assert.IsNotNull(result.warehouse);
        Assert.AreEqual(2, result.warehouse.WarehouseId); // ID should be reused
    }
}
