namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class InventoryItemResponseDto
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ItemType { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Description { get; set; }

    public decimal AvailableQuantity { get; set; }

    public decimal MinimumStock { get; set; }

    public bool IsLowStock { get; set; }

    public decimal PurchaseCost { get; set; }

    public decimal SalePrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}