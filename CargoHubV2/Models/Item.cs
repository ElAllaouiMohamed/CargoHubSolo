using CargohubV2.DataConverters;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CargohubV2.Models
{
    public class Item
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("uid")]
        [Required]
        [StringLength(50)]
        public string? UId { get; set; }

        [JsonProperty("code")]
        [Required]
        [StringLength(50)]
        public string? Code { get; set; }

        [JsonProperty("description")]
        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        [JsonProperty("short_description")]
        [StringLength(100)]
        public string? ShortDescription { get; set; }

        [JsonProperty("upc_code")]
        [StringLength(100)]
        public string? UpcCode { get; set; }

        [JsonProperty("model_number")]
        [StringLength(100)]
        public string? ModelNumber { get; set; }

        [JsonProperty("commodity_code")]
        [StringLength(100)]
        public string? CommodityCode { get; set; }

        [JsonProperty("item_line")]
        [Required]
        public int? ItemLineId { get; set; }

        [JsonProperty("item_group")]
        [Required]
        public int? ItemGroupId { get; set; }

        [JsonProperty("item_type")]
        [Required]
        public int? ItemTypeId { get; set; }

        [JsonProperty("unit_purchase_quantity")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "UnitPurchaseQuantity moet ≥ 0 zijn.")]
        public int UnitPurchaseQuantity { get; set; }

        [JsonProperty("unit_order_quantity")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "UnitOrderQuantity moet ≥ 0 zijn.")]
        public int UnitOrderQuantity { get; set; }

        [JsonProperty("pack_order_quantity")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "PackOrderQuantity moet ≥ 0 zijn.")]
        public int PackOrderQuantity { get; set; }

        [JsonProperty("supplier_id")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SupplierId moet een positief getal zijn.")]
        public int SupplierId { get; set; }

        [JsonProperty("supplier_code")]
        [Required]
        [StringLength(50)]
        public string? SupplierCode { get; set; }

        [JsonProperty("supplier_part_number")]
        [Required]
        [StringLength(50)]
        public string? SupplierPartNumber { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; } // Aanpasbaar gemaakt om CS0272 te vermijden

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UpdatedAt { get; set; } // Aanpasbaar gemaakt om CS0272 te vermijden

        [JsonProperty("weight_in_kg")]
        [Range(0, int.MaxValue, ErrorMessage = "WeightInKg moet ≥ 0 zijn.")]
        public int WeightInKg { get; set; }

        public bool IsDeleted { get; set; } = false;

        public Item_Line? ItemLine { get; set; }
        public Item_Group? ItemGroup { get; set; }
        public Item_Type? ItemType { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
