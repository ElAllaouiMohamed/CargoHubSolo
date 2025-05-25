using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace CargoHubV2.Models
{
    public class Order
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("source_id")]
        public int SourceId { get; set; }

        [JsonPropertyName("order_date")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("request_date")]
        public DateTime RequestDate { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("reference_extra")]
        public string? ReferenceExtra { get; set; }

        [JsonPropertyName("order_status")]
        public string? OrderStatus { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("shipping_notes")]
        public string? ShippingNotes { get; set; }

        [JsonPropertyName("picking_notes")]
        public string? PickingNotes { get; set; }

        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonPropertyName("ship_to")]
        public string? ShipTo { get; set; }

        [JsonPropertyName("bill_to")]
        public string? BillTo { get; set; }

        [JsonPropertyName("shipment_id")]
        public int ShipmentId { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("total_discount")]
        public decimal TotalDiscount { get; set; }

        [JsonPropertyName("total_tax")]
        public decimal TotalTax { get; set; }

        [JsonPropertyName("total_surcharge")]
        public decimal TotalSurcharge { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; } = new();

        public bool IsDeleted { get; set; } = false;
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        // Foreign key naar Order
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
