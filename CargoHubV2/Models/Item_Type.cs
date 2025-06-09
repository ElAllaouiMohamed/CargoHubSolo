using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Item_Type
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        [Required(ErrorMessage = "Name is verplicht.")]
        [StringLength(100, ErrorMessage = "Name mag maximaal 100 tekens zijn.")]
        public string Name { get; set; }

        [JsonProperty("description")]
        [StringLength(500, ErrorMessage = "Description mag maximaal 500 tekens zijn.")]
        public string Description { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
