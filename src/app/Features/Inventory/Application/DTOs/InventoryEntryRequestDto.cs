using System.ComponentModel.DataAnnotations;

namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class InventoryEntryRequestDto
{
    [Range(1, int.MaxValue)]
    public int InventoryItemId { get; set; }

    [Range(0.01, 99999999.99)]
    public decimal Quantity { get; set; }

    [Range(0.0, 9999999999.99)]
    public decimal UnitCost { get; set; }

    [Required]
    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}