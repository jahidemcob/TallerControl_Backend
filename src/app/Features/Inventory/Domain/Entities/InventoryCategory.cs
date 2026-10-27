namespace Backend.src.app.Features.Inventory.Domain.Entities;

public class InventoryCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Una categoría puede tener muchos artículos.
    public ICollection<InventoryItem> Items { get; set; }
        = new List<InventoryItem>();
}