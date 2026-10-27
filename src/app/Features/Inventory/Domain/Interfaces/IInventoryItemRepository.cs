using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;

namespace Backend.src.app.Features.Inventory.Domain.Interfaces;

public interface IInventoryItemRepository
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(
        int? categoryId = null,
        InventoryItemType? itemType = null,
        bool includeInactive = false,
        bool onlyLowStock = false,
        string? search = null);

    Task<InventoryItem?> GetByIdAsync(int id);

    Task<bool> ExistsByCodeAsync(
        string code,
        int? excludeId = null);

    Task<InventoryItem> CreateAsync(
        InventoryItem item);

    Task<InventoryItem?> UpdateAsync(
        int id,
        InventoryItem updatedItem);

    Task<bool> DeactivateAsync(int id);

    Task<bool> ActivateAsync(int id);
}