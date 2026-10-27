using Backend.src.app.Features.Inventory.Domain.Enums;

namespace Backend.src.app.Features.Inventory.Domain.Entities;

public class InventoryItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public InventoryItemType ItemType { get; set; }

    public string? Brand { get; set; }

    public string? Description { get; set; }

    public decimal AvailableQuantity { get; set; }

    public decimal MinimumStock { get; set; }

    public decimal PurchaseCost { get; set; }

    public decimal SalePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Categoría a la que pertenece el artículo.
    public InventoryCategory? Category { get; set; }

    // Historial de movimientos del artículo.
    public ICollection<InventoryMovement> Movements { get; set; }
        = new List<InventoryMovement>();

    // Registros de utilización en reparaciones.
    public ICollection<AppointmentInventoryItem> AppointmentItems { get; set; }
        = new List<AppointmentInventoryItem>();
}