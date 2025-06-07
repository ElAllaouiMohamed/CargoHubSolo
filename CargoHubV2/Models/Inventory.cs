using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class InventoryLocation
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("inventory_id")]
        public int InventoryId { get; set; }

        [Required]
        [JsonProperty("location_id")]
        public int LocationId { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public Inventory? Inventory { get; set; }
        public Location? Location { get; set; }
    }

    public class Inventory
    {
        public HazardClassification HazardClassification { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ItemId is verplicht.")]
        [JsonProperty("item_id")]
        public string? ItemId { get; set; }

        [Required(ErrorMessage = "Description is verplicht.")]
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("item_reference")]
        public string? ItemReference { get; set; }

        [JsonProperty("locations")]
        public List<int>? Locations { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalOnHand moet ≥ 0 zijn.")]
        [JsonProperty("total_on_hand")]
        public int TotalOnHand { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalExpected moet ≥ 0 zijn.")]
        [JsonProperty("total_expected")]
        public int TotalExpected { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalOrdered moet ≥ 0 zijn.")]
        [JsonProperty("total_ordered")]
        public int TotalOrdered { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalAllocated moet ≥ 0 zijn.")]
        [JsonProperty("total_allocated")]
        public int TotalAllocated { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalAvailable moet ≥ 0 zijn.")]
        [JsonProperty("total_available")]
        public int TotalAvailable { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        [Required(ErrorMessage = "InventoryLocations is verplicht.")]
        [JsonProperty("inventory_locations")]
        public ICollection<InventoryLocation> InventoryLocations { get; set; } = new List<InventoryLocation>();
    }
}
