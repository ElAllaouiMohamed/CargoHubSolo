using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Transfer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("from_location_id")]
        public int FromLocationId { get; set; }

        [JsonProperty("to_location_id")]
        public int ToLocationId { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
