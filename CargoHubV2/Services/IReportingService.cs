using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IReportingService
    {
        ReportResult GetWarehouseReport(int warehouseId);
        string GenerateCsvReport(int warehouseId);
        RevenueResult GetRevenueBetweenDates(DateTime start, DateTime end);
    }

}
