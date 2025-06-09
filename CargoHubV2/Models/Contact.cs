using CargohubV2.DataConverters;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;


namespace CargohubV2.Models
{
    public class Contact
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}

