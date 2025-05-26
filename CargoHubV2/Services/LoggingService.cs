using System;
using System.Threading.Tasks;
using CargohubV2.Models;
using CargohubV2.Contexts;

namespace CargohubV2.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly CargoHubDbContext _context;

        public LoggingService(CargoHubDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string user, string entity, string action, string endpoint, string details)
        {
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                User = user,
                Entity = entity,
                Action = action,
                Endpoint = endpoint,
                Details = details
            };
            _context.LogEntries.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
