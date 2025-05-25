using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Supplier
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("contact_name")]
        public string? ContactName { get; set; }

        [JsonProperty("contact_email")]
        public string? ContactEmail { get; set; }

        [JsonProperty("contact_phone")]
        public string? ContactPhone { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
