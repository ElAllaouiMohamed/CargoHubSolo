using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargohubV2.DataConverters;
using Newtonsoft.Json;

namespace CargohubV2.Models
{
    public class Shipment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
        [Required]
        public int OrderId { get; set; }

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

        [JsonProperty("shipment_date")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime ShipmentDate { get; set; }

        [JsonProperty("shipment_type")]
        [StringLength(50)]
        public string? ShipmentType { get; set; }

        [JsonProperty("shipment_status")]
        [StringLength(50)]
        public string? ShipmentStatus { get; set; }

        [JsonProperty("notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [JsonProperty("carrier_code")]
        [StringLength(100)]
        public string? CarrierCode { get; set; }

        [JsonProperty("carrier_description")]
        [StringLength(255)]
        public string? CarrierDescription { get; set; }

        [JsonProperty("service_code")]
        [StringLength(50)]
        public string? ServiceCode { get; set; }

        [JsonProperty("payment_type")]
        [StringLength(50)]
        public string? PaymentType { get; set; }

        [JsonProperty("transfer_mode")]
        [StringLength(50)]
        public string? TransferMode { get; set; }

        [JsonProperty("total_package_count")]
        [Range(0, int.MaxValue)]
        public int TotalPackageCount { get; set; }

        [JsonProperty("total_package_weight")]
        [Range(0, double.MaxValue)]
        public double TotalPackageWeight { get; set; }

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
        public List<ShipmentStock> Stocks { get; set; } = new List<ShipmentStock>();

        public bool IsDeleted { get; set; } = false;
    }
}
