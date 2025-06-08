using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Location
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("warehouse_id")]
        [Required(ErrorMessage = "WarehouseId is verplicht.")]
        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId moet een positief getal zijn.")]
        public int WarehouseId { get; set; }

        [JsonProperty("code")]
        [Required(ErrorMessage = "Code is verplicht.")]
        [StringLength(50, ErrorMessage = "Code mag maximaal 50 tekens zijn.")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        [Required(ErrorMessage = "Name is verplicht.")]
        [StringLength(100, ErrorMessage = "Name mag maximaal 100 tekens zijn.")]
        public string? Name { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
