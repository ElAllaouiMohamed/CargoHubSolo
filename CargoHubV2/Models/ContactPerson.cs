using CargohubV2.Models;

namespace CargohubV2.Models;
public class ContactPerson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Function { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    public int? WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    public int? ClientId { get; set; }
    public Client Client { get; set; }

    public int? SupplierId { get; set; }
    public Supplier Supplier { get; set; }
}
