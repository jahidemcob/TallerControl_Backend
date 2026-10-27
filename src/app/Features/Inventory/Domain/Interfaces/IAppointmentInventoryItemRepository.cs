using Backend.src.app.Features.Inventory.Domain.Entities;

namespace Backend.src.app.Features.Inventory.Domain.Interfaces;

public interface IAppointmentInventoryItemRepository
{
    Task<IReadOnlyList<AppointmentInventoryItem>> GetByAppointmentIdAsync(
        int appointmentId);

    Task<AppointmentInventoryItem?> GetByIdAsync(long id);

    Task<bool> ExistsByAppointmentAndItemAsync(
        int appointmentId,
        int inventoryItemId);

    Task<bool> HasProcessedItemsAsync(int appointmentId);

    Task<AppointmentInventoryItem> CreateAsync(
        AppointmentInventoryItem item);

    Task<AppointmentInventoryItem?> UpdateQuantityAsync(
        int appointmentId,
        long id,
        decimal quantity);

    Task<bool> DeletePendingAsync(int appointmentId, long id);
}