using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Warehouse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("hazmat_level")]
        public string? HazmatLevel { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
