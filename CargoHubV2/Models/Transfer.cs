using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargohubV2.DataConverters;
using Newtonsoft.Json;

namespace CargohubV2.Models
{
    public class Transfer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reference")]
        [Required]
        [StringLength(100)]
        public string? Reference { get; set; }

        [JsonProperty("transfer_from")]
        public int TransferFrom { get; set; }

        [JsonProperty("transfer_to")]
        public int TransferTo { get; set; }

        [JsonProperty("transfer_status")]
        [StringLength(50)]
        public string? TransferStatus { get; set; }

        [JsonProperty("created_at")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("items")]
        public List<TransferStock>? Stocks { get; set; } = new List<TransferStock>();

        public bool IsDeleted { get; set; } = false;
    }
}
