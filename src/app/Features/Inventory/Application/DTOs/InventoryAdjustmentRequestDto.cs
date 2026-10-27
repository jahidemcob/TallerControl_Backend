using System.ComponentModel.DataAnnotations;

namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class InventoryAdjustmentRequestDto
{
    [Range(1, int.MaxValue)]
    public int InventoryItemId { get; set; }

    [Range(0.01, 99999999.99)]
    public decimal Quantity { get; set; }

    [Required]
    public bool? IncreaseStock { get; set; }

    [Required]
    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}