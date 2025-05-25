using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Item
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("short_description")]
        public string? ShortDescription { get; set; }

        [JsonProperty("supplier_id")]
        public int SupplierId { get; set; }

        [JsonProperty("unit_order_quantity")]
        public int UnitOrderQuantity { get; set; }

        [JsonProperty("unit_purchase_quantity")]
        public int UnitPurchaseQuantity { get; set; }

        [JsonProperty("pack_order_quantity")]
        public int PackOrderQuantity { get; set; }

        [JsonProperty("hazmat_level")]
        public string? HazmatLevel { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
