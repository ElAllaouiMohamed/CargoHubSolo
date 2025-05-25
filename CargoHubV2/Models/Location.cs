using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Location
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
