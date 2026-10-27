using System.Data;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;
using Backend.src.app.Features.Inventory.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.src.app.Features.Inventory.Infrastructure.Repository;

public class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly InventoryDbContext _context;

    private const decimal MaxStock = 99999999.99m;

    public InventoryMovementRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetAllAsync(
        int? inventoryItemId = null,
        InventoryMovementType? movementType = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        IQueryable<InventoryMovement> query =
            _context.InventoryMovements
                .AsNoTracking()
                .Include(movement => movement.InventoryItem);

        if (inventoryItemId.HasValue)
        {
            query = query.Where(
                movement =>
                    movement.InventoryItemId == inventoryItemId.Value);
        }

        if (movementType.HasValue)
        {
            query = query.Where(
                movement =>
                    movement.MovementType == movementType.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(
                movement => movement.MovementDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(
                movement => movement.MovementDate <= to.Value);
        }

        return await query
            .OrderByDescending(movement => movement.MovementDate)
            .ThenByDescending(movement => movement.Id)
            .ToListAsync();
    }

    public async Task<InventoryMovement?> GetByIdAsync(long id)
    {
        return await _context.InventoryMovements
            .AsNoTracking()
            .Include(movement => movement.InventoryItem)
            .FirstOrDefaultAsync(movement => movement.Id == id);
    }

    public async Task<InventoryMovement?> RegisterAndUpdateStockAsync(
        InventoryMovement movement,
        decimal quantityDelta,
        decimal? newPurchaseCost = null)
    {
        var executionStrategy =
            _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(
                    item => item.Id == movement.InventoryItemId);

            if (item is null || !item.IsActive)
            {
                await transaction.RollbackAsync();
                return null;
            }

            var updatedQuantity =
                item.AvailableQuantity + quantityDelta;

            if (updatedQuantity < 0m ||
                updatedQuantity > MaxStock)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Actualizar las existencias.
            item.AvailableQuantity = updatedQuantity;

            // En una entrada, actualizar el último costo de compra.
            if (newPurchaseCost.HasValue)
            {
                item.PurchaseCost = newPurchaseCost.Value;
            }

            // Vincular el artículo con el movimiento.
            movement.InventoryItem = item;

            _context.InventoryMovements.Add(movement);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return movement;
        });
    }

    public async Task<bool> ProcessAppointmentInventoryAsync(
    int appointmentId)
    {
        var executionStrategy =
            _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var pendingLines = await _context.AppointmentInventoryItems
                .Where(line =>
                    line.AppointmentId == appointmentId &&
                    !line.InventoryProcessed)
                .OrderBy(line => line.InventoryItemId)
                .ToListAsync();

            // Si no hay consumos pendientes, no hay nada que descontar.
            // Esto permite completar reparaciones que no utilizaron repuestos.
            if (pendingLines.Count == 0)
            {
                await transaction.CommitAsync();
                return true;
            }

            // Agrupar cantidades por artículo para validar el stock total
            // necesario antes de descontar cualquier unidad.
            var requiredByItem = pendingLines
                .GroupBy(line => line.InventoryItemId)
                .Select(group => new
                {
                    InventoryItemId = group.Key,
                    Quantity = group.Sum(line => line.Quantity)
                })
                .ToList();

            var itemIds = requiredByItem
                .Select(group => group.InventoryItemId)
                .ToList();

            var items = await _context.InventoryItems
                .Where(item => itemIds.Contains(item.Id))
                .OrderBy(item => item.Id)
                .ToDictionaryAsync(item => item.Id);

            // Primera fase: validar TODOS los artículos.
            // Si alguno falla, no se modifica ni se registra ninguno.
            foreach (var required in requiredByItem)
            {
                if (!items.TryGetValue(
                        required.InventoryItemId,
                        out var item) ||
                    !item.IsActive ||
                    item.AvailableQuantity < required.Quantity)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            var processedAt = DateTime.UtcNow;

            // Segunda fase: descontar existencias y registrar movimientos.
            foreach (var line in pendingLines)
            {
                var item = items[line.InventoryItemId];

                item.AvailableQuantity -= line.Quantity;

                line.InventoryProcessed = true;
                line.InventoryProcessedAt = processedAt;

                var movement = new InventoryMovement
                {
                    InventoryItemId = item.Id,
                    MovementType = InventoryMovementType.Salida,
                    Quantity = line.Quantity,
                    UnitCost = line.UnitCost,
                    MovementDate = processedAt,
                    Reason =
                        $"Consumo de reparación de la cita #{appointmentId}",
                    AppointmentId = appointmentId,
                    UserId = line.UserId,
                    AppointmentInventoryItemId = line.Id,
                    InventoryItem = item,
                    AppointmentInventoryItem = line
                };

                _context.InventoryMovements.Add(movement);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        });
    }
}