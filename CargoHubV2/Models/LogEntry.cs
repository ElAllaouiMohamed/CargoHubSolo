using System;

namespace CargohubV2.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; } = "system";
        public string Entity { get; set; }
        public string Action { get; set; }
        public string Endpoint { get; set; }
        public string Details { get; set; }
    }
}

