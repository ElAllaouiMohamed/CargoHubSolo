namespace UnitTests;
using CargoHubV2.Data;
using CargoHubV2;
using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[TestClass]
public class UnitTest_Transfers
{
    private CargoHubDbContext _dbContext;
    private TransferService _transferService;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CargoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CargoHubDbContext(options);
        SeedDatabase(_dbContext);
        _transferService = new TransferService(_dbContext);
    }

    private void SeedDatabase(CargoHubDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.Locations.AddRange(new List<Location>
        {
            new Location
            {
                LocationId = 1,
                WarehouseId = 1,
                Code = "LOC001",
                Name = "Source Location",
                MaxWeight = 100,
                MaxWidth = 10,
                MaxHeight = 10,
                MaxDepth = 10,
                IsDock = false,
                ItemAmounts = new Dictionary<string, int> { { "ITEM001", 10 }, { "ITEM002", 5 }, { "ITEM004", 2 } },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Location
            {
                LocationId = 2,
                WarehouseId = 1,
                Code = "LOC002",
                Name = "Destination Location",
                MaxWeight = 100,
                MaxWidth = 10,
                MaxHeight = 10,
                MaxDepth = 10,
                IsDock = false,
                ItemAmounts = new Dictionary<string, int>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Location
            {
                LocationId = 3,
                WarehouseId = 2,
                Code = "LOC003",
                Name = "Other Warehouse Location",
                MaxWeight = 100,
                MaxWidth = 10,
                MaxHeight = 10,
                MaxDepth = 10,
                IsDock = false,
                ItemAmounts = new Dictionary<string, int>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Location
            {
                LocationId = 4,
                WarehouseId = 1,
                Code = "LOC004",
                Name = "No stock Location",
                MaxWeight = 10000,
                MaxWidth = 10000,
                MaxHeight = 10000,
                MaxDepth = 10000,
                IsDock = false,
                ItemAmounts = new Dictionary<string, int> { { "ITEM001", 1 }, { "ITEM002", 1 } },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        });

        context.Items.AddRange(new List<Item>
        {
            new Item
            {
                Uid = "ITEM001",
                Code = "CODE001",
                Description = "Item 001 Description",
                ShortDescription = "Item 001",
                UpcCode = "UPC001",
                ModelNumber = "MOD001",
                CommodityCode = "COM001",
                SupplierCode = "SUP001",
                SupplierPartNumber = "PART001",
                Height = 1,
                Width = 1,
                Depth = 1,
                Weight = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Item
            {
                Uid = "ITEM002",
                Code = "CODE002",
                Description = "Item 002 Description",
                ShortDescription = "Item 002",
                UpcCode = "UPC002",
                ModelNumber = "MOD002",
                CommodityCode = "COM002",
                SupplierCode = "SUP002",
                SupplierPartNumber = "PART002",
                Height = 1,
                Width = 1,
                Depth = 1,
                Weight = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Item
            {
                Uid = "ITEM003",
                Code = "CODE003",
                Description = "Item 003 Description",
                ShortDescription = "Item 003",
                UpcCode = "UPC003",
                ModelNumber = "MOD003",
                CommodityCode = "COM003",
                SupplierCode = "SUP003",
                SupplierPartNumber = "PART003",
                Height = 5,
                Width = 5,
                Depth = 5,
                Weight = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Item
            {
                Uid = "ITEM004",
                Code = "CODE004",
                Description = "Too Large Item",
                ShortDescription = "Item 004",
                UpcCode = "UPC004",
                ModelNumber = "MOD004",
                CommodityCode = "COM004",
                SupplierCode = "SUP004",
                SupplierPartNumber = "PART004",
                Height = 5000,
                Width = 5000,
                Depth = 5000,
                Weight = 10000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        });

        context.Transfers.AddRange(new List<Transfer>
        {
            new Transfer
            {
                TransferId = 1,
                Reference = "REF001",
                TransferFrom = 1,
                TransferTo = 2,
                TransferStatus = "Pending",
                Items = new List<TransferItem>
                {
                    new TransferItem { ItemId = "ITEM001", Amount = 2 },
                    new TransferItem { ItemId = "ITEM002", Amount = 1 }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Transfer
            {
                TransferId = 2,
                Reference = "REF002",
                TransferFrom = 999,
                TransferTo = 2,
                TransferStatus = "Pending",
                Items = new List<TransferItem>
                {
                    new TransferItem { ItemId = "ITEM001", Amount = 2 },
                    new TransferItem { ItemId = "ITEM002", Amount = 1 }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        });

        context.SaveChanges();
    }

    [TestMethod]
    public async Task TestGetAllTransfers()
    {
        var transfers = await _transferService.GetAllTransfersAsync();
        Assert.AreEqual(2, transfers.Count);
    }

    [TestMethod]
    public async Task TestGetTransferById_ValidId()
    {
        var transfer = await _transferService.GetTransferByIdAsync(1);
        Assert.IsNotNull(transfer);
        Assert.AreEqual("REF001", transfer.Reference);
    }

    [TestMethod]
    public async Task TestGetTransferById_InvalidId()
    {
        var transfer = await _transferService.GetTransferByIdAsync(999);
        Assert.IsNull(transfer);
    }

    [TestMethod]
    public async Task TestAddTransfer_ValidData()
    {
        var transfer = new Transfer
        {
            Reference = "REF003",
            TransferFrom = 1,
            TransferTo = 2,
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "ITEM001", Amount = 2 },
                new TransferItem { ItemId = "ITEM002", Amount = 3 }
            }
        };

        var result = await _transferService.AddTransferAsync(transfer);
        Assert.AreEqual("Transfer successfully created.", result.message);
        Assert.IsNotNull(result.transfer);
        Assert.AreEqual(3, result.transfer.TransferId);
    }

    [TestMethod]
    public async Task TestAddTransfer_InvalidItem()
    {
        var transfer = new Transfer
        {
            Reference = "REF003",
            TransferFrom = 1,
            TransferTo = 2,
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "INVALID_ITEM", Amount = 1 }
            }
        };

        var result = await _transferService.AddTransferAsync(transfer);
        Assert.AreEqual("Item INVALID_ITEM does not exist.", result.message);
        Assert.IsNull(result.transfer);
    }

    [TestMethod]
    public async Task TestUpdateTransferStatus_Valid()
    {
        var result = await _transferService.UpdateTransferStatusAsync(1, "InProgress");
        Assert.AreEqual("Transfer status successfully updated.", result);

        var transfer = await _transferService.GetTransferByIdAsync(1);
        Assert.IsNotNull(transfer);
        Assert.AreEqual("InProgress", transfer.TransferStatus);
    }

    [TestMethod]
    public async Task TestUpdateTransferStatus_InvalidId()
    {
        var result = await _transferService.UpdateTransferStatusAsync(999, "InProgress");
        Assert.AreEqual("Transfer not found.", result);
    }

    [TestMethod]
    public async Task TestDeleteTransfer_ValidId()
    {
        var result = await _transferService.DeleteTransferAsync(1);
        Assert.AreEqual("Transfer successfully deleted.", result);

        var transfer = await _transferService.GetTransferByIdAsync(1);
        Assert.IsNull(transfer);
    }

    [TestMethod]
    public async Task TestDeleteTransfer_InvalidId()
    {
        var result = await _transferService.DeleteTransferAsync(999);
        Assert.AreEqual("Transfer not found.", result);
    }

    [TestMethod]
    public async Task TestAddTransfer_ReusesDeletedId()
    {
        await _transferService.DeleteTransferAsync(1);

        var transfer = new Transfer
        {
            Reference = "REF004",
            TransferFrom = 1,
            TransferTo = 2,
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "ITEM001", Amount = 1 }
            }
        };

        var result = await _transferService.AddTransferAsync(transfer);
        Assert.AreEqual("Transfer successfully created.", result.message);
        Assert.IsNotNull(result.transfer);
        Assert.AreEqual(1, result.transfer.TransferId); // ID should be reused
    }

    [TestMethod]
    public async Task TestAddTransfer_AcrossDifferentWarehouses()
    {
        var transfer = new Transfer
        {
            Reference = "REF005",
            TransferFrom = 1,
            TransferTo = 3, // Different warehouse location
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "ITEM001", Amount = 1 }
            }
        };

        var result = await _transferService.AddTransferAsync(transfer);
        Assert.AreEqual("Transfers must remain within the same warehouse.", result.message);
        Assert.IsNull(result.transfer);
    }

    [TestMethod]
    public async Task TestAddTransfer_ExceedingDestinationConstraints()
    {
        var transfer = new Transfer
        {
            Reference = "REF006",
            TransferFrom = 1,
            TransferTo = 2,
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "ITEM004", Amount = 1 } // Exceeds constraints
            }
        };

        var result = await _transferService.AddTransferAsync(transfer);
        Assert.AreEqual($"Item ITEM004 exceeds destination location's constraints.", result.message);
        Assert.IsNull(result.transfer);
    }

    [TestMethod]
    public async Task TestUpdateTransfer_InvalidData()
    {
        var transfer = new Transfer
        {
            Reference = "REF007",
            TransferFrom = 999, // Invalid source location
            TransferTo = 2,
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "ITEM001", Amount = 1 }
            }
        };

        var result = await _transferService.UpdateTransferAsync(1, transfer);
        Assert.AreEqual("Invalid source or destination location.", result.message);
        Assert.IsNull(result.transfer);
    }

    [TestMethod]
    public async Task TestUpdateTransferStatus_SourceLocationMissing()
    {
        var result = await _transferService.UpdateTransferStatusAsync(2, "InProgress");
        Assert.AreEqual("Source location not found.", result);
    }

    [TestMethod]
    public async Task TestUpdateTransferStatus_InsufficientStock()
    {
        var transfer = new Transfer
        {
            Reference = "REF010",
            TransferFrom = 4,
            TransferTo = 1,
            Items = new List<TransferItem>
            {
                new TransferItem { ItemId = "ITEM001", Amount = 15 } // More than available stock
            }
        };

        var addResult = await _transferService.AddTransferAsync(transfer);
        Assert.AreEqual($"Not enough stock for item ITEM001 in the source location.", addResult.message);
        Assert.IsNull(addResult.transfer);
    }

    [TestMethod]
    public async Task TestCompleteTransfer_StockUpdated()
    {
        await _transferService.UpdateTransferStatusAsync(1, "InProgress");
        var result = await _transferService.UpdateTransferStatusAsync(1, "Completed");
        Assert.AreEqual("Transfer status successfully updated.", result);

        var toLocation = await _dbContext.Locations.FirstOrDefaultAsync(l => l.LocationId == 2);
        Assert.IsNotNull(toLocation);
        Assert.AreEqual(2, toLocation.ItemAmounts["ITEM001"]);
        Assert.AreEqual(1, toLocation.ItemAmounts["ITEM002"]);
    }

    [TestMethod]
    public async Task WrongTransferStatusTransition()
    {
        var result = await _transferService.UpdateTransferStatusAsync(1, "Completed");
        Assert.AreEqual("Invalid status transition or status. You can only update a transfer from 'Pending' to 'InProgress', or 'InProgress' to 'Completed'.", result);
    }

    [TestMethod]
    public async Task TestDeleteTransfer_Incomplete()
    {
        await _transferService.DeleteTransferAsync(1);

        var fromLocation = await _dbContext.Locations.FirstOrDefaultAsync(l => l.LocationId == 1);
        Assert.IsNotNull(fromLocation);
        Assert.AreEqual(12, fromLocation.ItemAmounts["ITEM001"]); // Restored stock
        Assert.AreEqual(6, fromLocation.ItemAmounts["ITEM002"]);
    }
}
