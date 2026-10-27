namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class AppointmentInventoryItemResponseDto
{
    public long Id { get; set; }

    public int AppointmentId { get; set; }

    public int InventoryItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public int UserId { get; set; }

    public DateTime RecordedAt { get; set; }

    public bool InventoryProcessed { get; set; }

    public DateTime? InventoryProcessedAt { get; set; }
}