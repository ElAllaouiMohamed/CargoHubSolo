using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CargoHubV2.Models
{
    public class Shipment
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        [JsonPropertyName("source_id")]
        public int SourceId { get; set; }

        [JsonPropertyName("order_date")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("request_date")]
        public DateTime RequestDate { get; set; }

        [JsonPropertyName("shipment_date")]
        public DateTime ShipmentDate { get; set; }

        [JsonPropertyName("shipment_type")]
        public string? ShipmentType { get; set; }

        [JsonPropertyName("shipment_status")]
        public string? ShipmentStatus { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("carrier_code")]
        public string? CarrierCode { get; set; }

        [JsonPropertyName("carrier_description")]
        public string? CarrierDescription { get; set; }

        [JsonPropertyName("service_code")]
        public string? ServiceCode { get; set; }

        [JsonPropertyName("payment_type")]
        public string? PaymentType { get; set; }

        [JsonPropertyName("transfer_mode")]
        public string? TransferMode { get; set; }

        [JsonPropertyName("total_package_count")]
        public int TotalPackageCount { get; set; }

        [JsonPropertyName("total_package_weight")]
        public double TotalPackageWeight { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("items")]
        public List<ShipmentItem> Items { get; set; } = new();

        public bool IsDeleted { get; set; } = false;
    }
    public class ShipmentItem
    {
        [Key]
        public int Id { get; set; }

        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [ForeignKey("Shipment")]
        public int ShipmentId { get; set; }

        [JsonIgnore]
        public Shipment? Shipment { get; set; }
    }
}
