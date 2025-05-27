using CargohubV2.DataConverters;
using Newtonsoft.Json;
using CargohubV2.Models;

namespace CargohubV2.Models
{
    public class Warehouse
    {
        public ICollection<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();

        public HazardClassification HazardClassification { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("zip")]
        public string? Zip { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("province")]
        public string? Province { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }


}

