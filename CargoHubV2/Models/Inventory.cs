using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Inventory
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("location_id")]
        public int LocationId { get; set; }

        [JsonProperty("total_on_hand")]
        public int TotalOnHand { get; set; }

        [JsonProperty("total_available")]
        public int TotalAvailable { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
