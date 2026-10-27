using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;

namespace Backend.src.app.Features.Inventory.Domain.Interfaces;

public interface IInventoryMovementRepository
{
    Task<IReadOnlyList<InventoryMovement>> GetAllAsync(
        int? inventoryItemId = null,
        InventoryMovementType? movementType = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<InventoryMovement?> GetByIdAsync(long id);

    Task<InventoryMovement?> RegisterAndUpdateStockAsync(
        InventoryMovement movement,
        decimal quantityDelta,
        decimal? newPurchaseCost = null);

    Task<bool> ProcessAppointmentInventoryAsync(int appointmentId);
}