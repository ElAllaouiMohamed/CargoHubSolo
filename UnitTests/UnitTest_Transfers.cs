using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargohubV2.Contexts;
using CargohubV2.Models;
using CargohubV2.Services;

namespace UnitTests
{
    [TestClass]
    public class TransferServiceTests
    {
        private CargoHubDbContext _dbContext = null!;
        private TransferService _transferService = null!;
        private Mock<LoggingService> _mockLoggingService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CargoHubDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCargoHubDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new CargoHubDbContext(options);
            _mockLoggingService = new Mock<LoggingService>();
            _transferService = new TransferService(_dbContext, _mockLoggingService.Object);

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
            context.Transfers.AddRange(
                new Transfer
                {
                    Id = 1,
                    Reference = "TRF001",
                    TransferFrom = 10,
                    TransferTo = 20,
                    TransferStatus = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    IsDeleted = false,
                    Stocks = new List<TransferStock> {
                        new TransferStock { Id = 1, TransferId = 1, ItemId = "1", Quantity = 5 }
                    }
                },
                new Transfer
                {
                    Id = 2,
                    Reference = "TRF002",
                    TransferFrom = 11,
                    TransferTo = 21,
                    TransferStatus = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    IsDeleted = false,
                    Stocks = new List<TransferStock> {
                        new TransferStock { Id = 2, TransferId = 2, ItemId = "2", Quantity = 10 }
                    }
                },
                new Transfer
                {
                    Id = 3,
                    Reference = "TRF003",
                    TransferFrom = 12,
                    TransferTo = 22,
                    TransferStatus = "Cancelled",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3),
                    IsDeleted = true // deleted transfer
                }
            );

            context.SaveChanges();
        }

        [TestMethod]
        public async Task GetTransfersAsync_ShouldReturnLimitedNonDeletedTransfers()
        {
            var transfers = await _transferService.GetTransfersAsync(2);
            Assert.AreEqual(2, transfers.Count);
            Assert.IsTrue(transfers.All(t => !t.IsDeleted));
            Assert.AreEqual("TRF001", transfers[0].Reference);
            Assert.AreEqual("TRF002", transfers[1].Reference);
            Assert.AreEqual(1, transfers[0].Stocks.Count);
            Assert.AreEqual(5, transfers[0].Stocks[0].Quantity);
        }

        [TestMethod]
        public async Task GetAllTransfersAsync_ShouldReturnAllNonDeletedTransfers()
        {
            var transfers = await _transferService.GetAllTransfersAsync();
            Assert.AreEqual(2, transfers.Count);
            Assert.IsTrue(transfers.All(t => !t.IsDeleted));
            Assert.IsTrue(transfers.Any(t => t.Reference == "TRF001"));
            Assert.IsTrue(transfers.Any(t => t.Reference == "TRF002"));
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnTransfer()
        {
            var transfer = await _transferService.GetByIdAsync(1);
            Assert.IsNotNull(transfer);
            Assert.AreEqual("TRF001", transfer.Reference);
            Assert.IsFalse(transfer.IsDeleted);
            Assert.AreEqual(1, transfer.Stocks.Count);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var transfer = await _transferService.GetByIdAsync(999);
            Assert.IsNull(transfer);
        }

        [TestMethod]
        public async Task AddTransferAsync_ShouldAddAndLog()
        {
            var newTransfer = new Transfer
            {
                Reference = "TRF004",
                TransferFrom = 15,
                TransferTo = 25,
                TransferStatus = "New",
                Stocks = new List<TransferStock>
                {
                    new TransferStock { ItemId = "3", Quantity = 8 }
                }
            };

            var added = await _transferService.AddTransferAsync(newTransfer);
            Assert.IsNotNull(added);
            Assert.AreEqual("TRF004", added.Reference);
            Assert.IsFalse(added.IsDeleted);
            Assert.IsTrue(added.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(added.UpdatedAt > DateTime.MinValue);
            Assert.AreEqual(1, added.Stocks.Count);
            Assert.AreEqual(8, added.Stocks[0].Quantity);

            var dbEntity = await _dbContext.Transfers.FindAsync(added.Id);
            Assert.IsNotNull(dbEntity);
            Assert.AreEqual("TRF004", dbEntity.Reference);

            _mockLoggingService.Verify(ls => ls.LogAsync(
                "system", "Transfer", "Create", "/api/v1/transfers", $"Created transfer {added.Id}"), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTransferAsync_ExistingId_ShouldUpdateAndLog()
        {
            var updated = new Transfer
            {
                Reference = "TRF001-Updated",
                TransferFrom = 10,
                TransferTo = 20,
                TransferStatus = "Updated",
                Stocks = new List<TransferStock> {
                    new TransferStock { ItemId = "1", Quantity = 10 }
                }
            };

            var result = await _transferService.UpdateTransferAsync(1, updated);
            Assert.IsNotNull(result);
            Assert.AreEqual("TRF001-Updated", result.Reference);
            Assert.AreEqual("Updated", result.TransferStatus);
            Assert.IsTrue(result.UpdatedAt > result.CreatedAt);
            Assert.AreEqual(1, result.Stocks.Count);
            Assert.AreEqual(10, result.Stocks[0].Quantity);

            var dbEntity = await _dbContext.Transfers.Include(t => t.Stocks).FirstOrDefaultAsync(t => t.Id == 1);
            Assert.IsNotNull(dbEntity);
            Assert.AreEqual("TRF001-Updated", dbEntity.Reference);

            _mockLoggingService.Verify(ls => ls.LogAsync(
                "system", "Transfer", "Update", "/api/v1/transfers/1", "Updated transfer 1"), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTransferAsync_NonExistingId_ShouldReturnNull()
        {
            var updated = new Transfer
            {
                Reference = "TRF999",
                TransferFrom = 10,
                TransferTo = 20,
                TransferStatus = "Updated"
            };

            var result = await _transferService.UpdateTransferAsync(999, updated);
            Assert.IsNull(result);

            _mockLoggingService.Verify(ls => ls.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_ExistingId_ShouldSoftDeleteAndLog()
        {
            var result = await _transferService.SoftDeleteByIdAsync(1);
            Assert.IsTrue(result);

            var transfer = await _dbContext.Transfers.FindAsync(1);
            Assert.IsNotNull(transfer);
            Assert.IsTrue(transfer.IsDeleted);
            Assert.IsTrue(transfer.UpdatedAt > transfer.CreatedAt);

            _mockLoggingService.Verify(ls => ls.LogAsync(
                "system", "Transfer", "Delete", "/api/v1/transfers/1", "Soft deleted transfer 1"), Times.Once);
        }

        [TestMethod]
        public async Task SoftDeleteByIdAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _transferService.SoftDeleteByIdAsync(999);
            Assert.IsFalse(result);

            _mockLoggingService.Verify(ls => ls.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
