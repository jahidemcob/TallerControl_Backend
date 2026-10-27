using Backend.src.app.Features.Inventory.Domain.Entities;

namespace Backend.src.app.Features.Inventory.Domain.Interfaces;

public interface IInventoryCategoryRepository
{
    Task<IReadOnlyList<InventoryCategory>> GetAllAsync(
        bool includeInactive = false);

    Task<InventoryCategory?> GetByIdAsync(int id);

    Task<bool> ExistsByNameAsync(
        string name,
        int? excludeId = null);

    Task<InventoryCategory> CreateAsync(
        InventoryCategory category);

    Task<InventoryCategory?> UpdateAsync(
        int id,
        InventoryCategory updatedCategory);

    Task<bool> DeactivateAsync(int id);

    Task<bool> HasActiveItemsAsync(int categoryId);

    Task<bool> ActivateAsync(int id);
}