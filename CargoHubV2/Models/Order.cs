using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Order
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("source_id")]
        [Required]
        public int SourceId { get; set; }

        [JsonProperty("order_date")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime OrderDate { get; set; }

        [JsonProperty("request_date")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime RequestDate { get; set; }

        [JsonProperty("reference")]
        [Required, StringLength(50)]
        public string? Reference { get; set; }

        [JsonProperty("reference_extra")]
        [StringLength(255)]
        public string? Reference_extra { get; set; }

        [JsonProperty("order_status")]
        [StringLength(50)]
        public string? Order_status { get; set; }

        [JsonProperty("notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [JsonProperty("shipping_notes")]
        [StringLength(1000)]
        public string? ShippingNotes { get; set; }

        [JsonProperty("picking_notes")]
        [StringLength(1000)]
        public string? PickingNotes { get; set; }

        [JsonProperty("warehouse_id")]
        [Required]
        public int WarehouseId { get; set; }

        [JsonProperty("ship_to")]
        [StringLength(255)]
        public string? ShipTo { get; set; }

        [JsonProperty("bill_to")]
        [StringLength(255)]
        public string? BillTo { get; set; }

        [JsonProperty("shipment_id")]
        [Required]
        public int ShipmentId { get; set; }

        [JsonProperty("total_amount")]
        [Range(0, double.MaxValue)]
        public double TotalAmount { get; set; }

        [JsonProperty("total_discount")]
        [Range(0, double.MaxValue)]
        public double TotalDiscount { get; set; }

        [JsonProperty("total_tax")]
        [Range(0, double.MaxValue)]
        public double TotalTax { get; set; }

        [JsonProperty("total_surcharge")]
        [Range(0, double.MaxValue)]
        public double TotalSurcharge { get; set; }

        [JsonProperty("created_at")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("items")]
        [MinLength(1)]
        public List<OrderStock> Stocks { get; set; } = new();

        public bool IsDeleted { get; set; } = false;
    }
}
