using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Interfaces;
using Backend.src.app.Features.Inventory.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.src.app.Features.Inventory.Infrastructure.Repository;

public class AppointmentInventoryItemRepository
    : IAppointmentInventoryItemRepository
{
    private readonly InventoryDbContext _context;

    public AppointmentInventoryItemRepository(
        InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AppointmentInventoryItem>>
        GetByAppointmentIdAsync(int appointmentId)
    {
        return await _context.AppointmentInventoryItems
            .AsNoTracking()
            .Include(line => line.InventoryItem)
            .Where(line => line.AppointmentId == appointmentId)
            .OrderBy(line => line.RecordedAt)
            .ToListAsync();
    }

    public async Task<AppointmentInventoryItem?> GetByIdAsync(long id)
    {
        return await _context.AppointmentInventoryItems
            .AsNoTracking()
            .Include(line => line.InventoryItem)
            .FirstOrDefaultAsync(line => line.Id == id);
    }

    public async Task<bool> ExistsByAppointmentAndItemAsync(
        int appointmentId,
        int inventoryItemId)
    {
        return await _context.AppointmentInventoryItems
            .AsNoTracking()
            .AnyAsync(line =>
                line.AppointmentId == appointmentId &&
                line.InventoryItemId == inventoryItemId);
    }

    public async Task<bool> HasProcessedItemsAsync(int appointmentId)
    {
        return await _context.AppointmentInventoryItems
            .AsNoTracking()
            .AnyAsync(line =>
                line.AppointmentId == appointmentId &&
                line.InventoryProcessed);
    }

    public async Task<AppointmentInventoryItem> CreateAsync(
        AppointmentInventoryItem item)
    {
        _context.AppointmentInventoryItems.Add(item);
        await _context.SaveChangesAsync();

        return await _context.AppointmentInventoryItems
            .AsNoTracking()
            .Include(line => line.InventoryItem)
            .FirstAsync(line => line.Id == item.Id);
    }

    public async Task<AppointmentInventoryItem?> UpdateQuantityAsync(
        int appointmentId,
        long id,
        decimal quantity)
    {
        var line = await _context.AppointmentInventoryItems
            .FirstOrDefaultAsync(line =>
                line.Id == id &&
                line.AppointmentId == appointmentId);

        if (line is null || line.InventoryProcessed)
        {
            return null;
        }

        line.Quantity = quantity;

        await _context.SaveChangesAsync();

        return await _context.AppointmentInventoryItems
            .AsNoTracking()
            .Include(line => line.InventoryItem)
            .FirstAsync(line => line.Id == id);
    }

    public async Task<bool> DeletePendingAsync(
        int appointmentId,
        long id)
    {
        var line = await _context.AppointmentInventoryItems
            .FirstOrDefaultAsync(line =>
                line.Id == id &&
                line.AppointmentId == appointmentId &&
                !line.InventoryProcessed);

        if (line is null)
        {
            return false;
        }

        _context.AppointmentInventoryItems.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }
}