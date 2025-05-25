using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Shipment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("carrier")]
        public string? Carrier { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("shipment_date")]
        public DateTime ShipmentDate { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
