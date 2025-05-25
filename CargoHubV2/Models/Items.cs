using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class Item
    {
        [JsonProperty("id")]
        public int Id { get; set; } 

        [JsonProperty("uid")]
        public string? Uid { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("short_description")]
        public string? ShortDescription { get; set; }

        [JsonProperty("upc_code")]
        public string? UpcCode { get; set; }

        [JsonProperty("model_number")]
        public string? ModelNumber { get; set; }

        [JsonProperty("commodity_code")]
        public string? CommodityCode { get; set; }

        [JsonProperty("item_line")]
        public int ItemLine { get; set; }

        [JsonProperty("item_group")]
        public int ItemGroup { get; set; }

        [JsonProperty("item_type")]
        public int ItemType { get; set; }

        [JsonProperty("unit_purchase_quantity")]
        public int UnitPurchaseQuantity { get; set; }

        [JsonProperty("unit_order_quantity")]
        public int UnitOrderQuantity { get; set; }

        [JsonProperty("pack_order_quantity")]
        public int PackOrderQuantity { get; set; }

        [JsonProperty("supplier_id")]
        public int SupplierId { get; set; }

        [JsonProperty("supplier_code")]
        public string? SupplierCode { get; set; }

        [JsonProperty("supplier_part_number")]
        public string? SupplierPartNumber { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
