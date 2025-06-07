using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Client
    {
        public ICollection<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();
        public HazardClassification HazardClassification { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [JsonProperty("name")]
        public string? Name { get; set; }

        [Required]
        [JsonProperty("address")]
        public string? Address { get; set; }

        [Required]
        [JsonProperty("city")]
        public string? City { get; set; }

        [Required]
        [JsonProperty("zip_code")]
        public string? ZipCode { get; set; }

        [Required]
        [JsonProperty("province")]
        public string? Province { get; set; }

        [Required]
        [JsonProperty("country")]
        public string? Country { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Name mag alleen letters en spaties.")]
        [JsonProperty("contact_name")]
        public string? ContactName { get; set; }

        [Required]
        [RegularExpression(@"^\+?[0-9\s\-]+$", ErrorMessage = "ongeldige phone number.")]
        [JsonProperty("contact_phone")]
        public string? ContactPhone { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "ongeldige email address.")]
        [JsonProperty("contact_email")]
        public string? ContactEmail { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

}

