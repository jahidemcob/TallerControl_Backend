using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Interfaces;
using Backend.src.app.Features.Inventory.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.src.app.Features.Inventory.Infrastructure.Repository;

public class InventoryCategoryRepository : IInventoryCategoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryCategoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventoryCategory>> GetAllAsync(
        bool includeInactive = false)
    {
        IQueryable<InventoryCategory> query =
            _context.InventoryCategories.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(category => category.IsActive);
        }

        return await query
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public async Task<InventoryCategory?> GetByIdAsync(int id)
    {
        return await _context.InventoryCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludeId = null)
    {
        var normalizedName = name.Trim();

        IQueryable<InventoryCategory> query =
            _context.InventoryCategories
                .AsNoTracking()
                .Where(category => category.Name == normalizedName);

        if (excludeId.HasValue)
        {
            query = query.Where(
                category => category.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<InventoryCategory> CreateAsync(
        InventoryCategory category)
    {
        category.Name = category.Name.Trim();
        category.Description = NormalizeDescription(
            category.Description);

        category.IsActive = true;

        _context.InventoryCategories.Add(category);

        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<InventoryCategory?> UpdateAsync(
        int id,
        InventoryCategory updatedCategory)
    {
        var category = await _context.InventoryCategories
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category is null)
        {
            return null;
        }

        category.Name = updatedCategory.Name.Trim();
        category.Description = NormalizeDescription(
            updatedCategory.Description);

        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var category = await _context.InventoryCategories
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category is null)
        {
            return false;
        }

        category.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasActiveItemsAsync(int categoryId)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .AnyAsync(item =>
                item.CategoryId == categoryId &&
                item.IsActive);
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var category = await _context.InventoryCategories
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category is null)
        {
            return false;
        }

        category.IsActive = true;

        await _context.SaveChangesAsync();

        return true;
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}