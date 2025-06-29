using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Item_Group
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is verplicht.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Numbers and special characters are not allowed")]
        [JsonProperty("name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is verplicht.")]
        [JsonProperty("description")]
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

