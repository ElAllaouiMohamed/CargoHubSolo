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

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("zip_code")]
        public string? ZipCode { get; set; }

        [JsonProperty("province")]
        public string? Province { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Numbers and special characters are not allowed")]
        [JsonProperty("contact_name")]
        public string? ContactName { get; set; }

        [RegularExpression(@"^\+?(\d{1,4})?[\s\(\)-]?\d{1,4}[\s\(\)-]?\d{1,4}[\s\(\)-]?\d{1,4}$", ErrorMessage = "Please enter a valid phone number.")]
        [JsonProperty("contact_phone")]
        public string? ContactPhone { get; set; }

        [JsonProperty("contact_email")]
        [RegularExpression(@"^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Please Enter a valid email")]
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

