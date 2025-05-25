using Newtonsoft.Json;

namespace CargoHubV2.Models
{
    public class TransferItem
    {
        public int Id { get; set; } // << primaire sleutel

        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        public int TransferId { get; set; } // foreign key
        public Transfer? Transfer { get; set; }
    }


    public class Transfer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("transfer_from")]
        public int? TransferFrom { get; set; }

        [JsonProperty("transfer_to")]
        public int? TransferTo { get; set; }

        [JsonProperty("transfer_status")]
        public string? TransferStatus { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("items")]
        public List<TransferItem>? Items { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
