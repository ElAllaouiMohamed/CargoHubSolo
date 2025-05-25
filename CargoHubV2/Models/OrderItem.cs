using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class OrderItem
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
