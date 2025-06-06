using CargohubV2.DataConverters;
using Newtonsoft.Json;

namespace CargohubV2.Models
{
    public class Supplier
    {
        public ICollection<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();


        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("address_extra")]
        public string? AddressExtra { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("zip_code")]
        public string? ZipCode { get; set; }

        [JsonProperty("province")]
        public string? Province { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("contact_name")]
        public string? ContactName { get; set; }

        [JsonProperty("phonenumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}

