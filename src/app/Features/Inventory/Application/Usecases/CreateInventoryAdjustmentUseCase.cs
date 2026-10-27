using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class CreateInventoryAdjustmentUseCase
{
    private readonly IInventoryItemRepository _itemRepository;
    private readonly IInventoryMovementRepository _movementRepository;

    public CreateInventoryAdjustmentUseCase(
        IInventoryItemRepository itemRepository,
        IInventoryMovementRepository movementRepository)
    {
        _itemRepository = itemRepository;
        _movementRepository = movementRepository;
    }

    public async Task<InventoryMovementResponseDto> Execute(
        int userId,
        InventoryAdjustmentRequestDto dto)
    {
        if (userId <= 0)
        {
            throw new InventoryValidationException(
                "El usuario responsable no es válido.");
        }

        if (dto.Quantity <= 0)
        {
            throw new InventoryValidationException(
                "La cantidad del ajuste debe ser mayor que cero.");
        }

        if (!dto.IncreaseStock.HasValue)
        {
            throw new InventoryValidationException(
                "Debes indicar si el ajuste aumenta o disminuye las existencias.");
        }

        var reason = dto.Reason.Trim();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InventoryValidationException(
                "El motivo del ajuste es obligatorio.");
        }

        var item = await _itemRepository.GetByIdAsync(
            dto.InventoryItemId);

        if (item is null)
        {
            throw new InventoryNotFoundException(
                "Artículo de inventario", dto.InventoryItemId);
        }

        if (!item.IsActive)
        {
            throw new InventoryValidationException(
                "No se pueden ajustar artículos inactivos.");
        }

        var increaseStock = dto.IncreaseStock.Value;

        var movement = new InventoryMovement
        {
            InventoryItemId = dto.InventoryItemId,
            MovementType = increaseStock
                ? InventoryMovementType.AjustePositivo
                : InventoryMovementType.AjusteNegativo,
            Quantity = dto.Quantity,
            UnitCost = null,
            MovementDate = DateTime.UtcNow,
            Reason = reason,
            UserId = userId
        };

        var quantityDelta = increaseStock
            ? dto.Quantity
            : -dto.Quantity;

        var registered =
            await _movementRepository.RegisterAndUpdateStockAsync(
                movement,
                quantityDelta);

        if (registered is null)
        {
            throw new InventoryConflictException(
                increaseStock
                    ? "No fue posible aplicar el ajuste positivo. " +
                      "Verifica que la cantidad final sea válida."
                    : "No hay existencias suficientes para aplicar " +
                      "el ajuste negativo.");
        }

        return InventoryMapper.ToMovementResponse(registered);
    }
}