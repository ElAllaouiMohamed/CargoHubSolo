namespace CargohubV2.Models
{
    public class ApiKey
    {
        public int ApiKeyId { get; set; }
        public string Name { get; set; } = null!;
        public string KeyHash { get; set; } = null!;
    }
}

