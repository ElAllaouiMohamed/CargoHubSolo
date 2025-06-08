using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargohubV2.DataConverters;
using Newtonsoft.Json;

namespace CargohubV2.Models
{
    public class Supplier
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        [Required, StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("addressExtra")]
        public string? AddressExtra { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("zipCode")]
        public string? ZipCode { get; set; }

        [JsonProperty("province")]
        public string? Province { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("contactName")]
        public string? ContactName { get; set; }

        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("created_at")]
        [Required, JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [Required, JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; } = false;

        [JsonProperty("contactPersons")]
        public List<ContactPerson> ContactPersons { get; set; } = new();
    }
}
