using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class ItemLine
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
