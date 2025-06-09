using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Warehouse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        [Required(ErrorMessage = "Code is required")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name kan niet langer zijn dan 100 characters")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address kan niet langer zijn dan 200 characters")]
        public string? Address { get; set; }

        [JsonProperty("zip")]
        [Required(ErrorMessage = "ZIP is verplicht")]
        [RegularExpression(@"^\d{4}[A-Z]{2}$", ErrorMessage = "ZIP moet deze formaat hebben 1234AB")]
        public string? Zip { get; set; }

        [JsonProperty("city")]
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City kan niet langer zijn dan 100 characters")]
        public string? City { get; set; }

        [JsonProperty("province")]
        [StringLength(100, ErrorMessage = "Province kan niet langer zijn dan 100 characters")]
        public string? Province { get; set; }

        [JsonProperty("country")]
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country kan niet langer zijn dan 100 characters")]
        public string? Country { get; set; }

        [JsonProperty("contact")]
        [Required(ErrorMessage = "Contact is verplicht")]
        public Contact Contact { get; set; } = new Contact();

        [JsonProperty("created_at")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [Required]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();

        [Required(ErrorMessage = "HazardClassification moet worden opgegeven.")]
        public HazardClassification HazardClassification { get; set; }
    }
}
