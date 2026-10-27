namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class InventoryMovementResponseDto
{
    public long Id { get; set; }

    public int InventoryItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string MovementType { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal? UnitCost { get; set; }

    public DateTime MovementDate { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int? AppointmentId { get; set; }

    public int UserId { get; set; }

    public long? AppointmentInventoryItemId { get; set; }
}