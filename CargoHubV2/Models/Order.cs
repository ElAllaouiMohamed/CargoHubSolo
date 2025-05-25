using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Order
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("client_id")]
        public int ClientId { get; set; }

        [JsonProperty("shipment_id")]
        public int ShipmentId { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
