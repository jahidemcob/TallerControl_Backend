using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;
using Backend.src.app.Features.Inventory.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.src.app.Features.Inventory.Infrastructure.Repository;

public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly InventoryDbContext _context;

    public InventoryItemRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(
        int? categoryId = null,
        InventoryItemType? itemType = null,
        bool includeInactive = false,
        bool onlyLowStock = false,
        string? search = null)
    {
        IQueryable<InventoryItem> query =
            _context.InventoryItems
                .AsNoTracking()
                .Include(item => item.Category);

        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(
                item => item.CategoryId == categoryId.Value);
        }

        if (itemType.HasValue)
        {
            query = query.Where(
                item => item.ItemType == itemType.Value);
        }

        if (onlyLowStock)
        {
            query = query.Where(
                item => item.AvailableQuantity <= item.MinimumStock);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(item =>
                item.Code.Contains(term) ||
                item.Name.Contains(term) ||
                (item.Brand != null && item.Brand.Contains(term)));
        }

        return await query
            .OrderBy(item => item.Name)
            .ToListAsync();
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        int? excludeId = null)
    {
        var normalizedCode = code.Trim();

        IQueryable<InventoryItem> query =
            _context.InventoryItems
                .AsNoTracking()
                .Where(item => item.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(
                item => item.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<InventoryItem> CreateAsync(
        InventoryItem item)
    {
        item.Code = item.Code.Trim();
        item.Name = item.Name.Trim();
        item.Brand = NormalizeText(item.Brand);
        item.Description = NormalizeText(item.Description);

        // Las existencias iniciales deben registrarse
        // mediante un movimiento de entrada.
        item.AvailableQuantity = 0m;

        item.IsActive = true;

        _context.InventoryItems.Add(item);

        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<InventoryItem?> UpdateAsync(
        int id,
        InventoryItem updatedItem)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(item => item.Id == id);

        if (item is null)
        {
            return null;
        }

        item.CategoryId = updatedItem.CategoryId;
        item.Code = updatedItem.Code.Trim();
        item.Name = updatedItem.Name.Trim();
        item.ItemType = updatedItem.ItemType;
        item.Brand = NormalizeText(updatedItem.Brand);
        item.Description = NormalizeText(updatedItem.Description);
        item.MinimumStock = updatedItem.MinimumStock;
        item.PurchaseCost = updatedItem.PurchaseCost;
        item.SalePrice = updatedItem.SalePrice;

        // No se modifican aquí:
        // AvailableQuantity, IsActive ni CreatedAt.

        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(item => item.Id == id);

        if (item is null)
        {
            return false;
        }

        item.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(item => item.Id == id);

        if (item is null)
        {
            return false;
        }

        item.IsActive = true;

        await _context.SaveChangesAsync();

        return true;
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}