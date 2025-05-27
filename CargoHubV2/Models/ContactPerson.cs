using System.ComponentModel.DataAnnotations.Schema;

namespace CargohubV2.Models
{
    public class ContactPerson
    {
        public int Id { get; set; }

        public int? WarehouseId { get; set; }
        public int? ClientId { get; set; }
        public int? SupplierId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse? Warehouse { get; set; }

        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        public string Name { get; set; }
        public string Function { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

