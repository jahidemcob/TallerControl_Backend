using Backend.src.app.Features.Inventory.Domain.Enums;

namespace Backend.src.app.Features.Inventory.Domain.Entities;

public class InventoryMovement
{
    public long Id { get; set; }

    public int InventoryItemId { get; set; }

    public InventoryMovementType MovementType { get; set; }

    // Siempre positiva. El tipo determina si suma o resta.
    public decimal Quantity { get; set; }

    // Costo histórico opcional del movimiento.
    public decimal? UnitCost { get; set; }

    public DateTime MovementDate { get; set; }

    public string Reason { get; set; } = string.Empty;

    // Identificadores lógicos de otros módulos.
    public int? AppointmentId { get; set; }

    public int UserId { get; set; }

    public long? AppointmentInventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public AppointmentInventoryItem? AppointmentInventoryItem { get; set; }
}