using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class CreateInventoryExitUseCase
{
    private readonly IInventoryItemRepository _itemRepository;
    private readonly IInventoryMovementRepository _movementRepository;

    public CreateInventoryExitUseCase(
        IInventoryItemRepository itemRepository,
        IInventoryMovementRepository movementRepository)
    {
        _itemRepository = itemRepository;
        _movementRepository = movementRepository;
    }

    public async Task<InventoryMovementResponseDto> Execute(
        int userId,
        InventoryExitRequestDto dto)
    {
        if (userId <= 0)
        {
            throw new InventoryValidationException(
                "El usuario responsable no es válido.");
        }

        if (dto.Quantity <= 0)
        {
            throw new InventoryValidationException(
                "La cantidad debe ser mayor que cero.");
        }

        var reason = dto.Reason.Trim();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InventoryValidationException(
                "El motivo de la salida es obligatorio.");
        }

        var item = await _itemRepository.GetByIdAsync(
            dto.InventoryItemId);

        if (item is null)
        {
            throw new InventoryNotFoundException(
                "Artículo de inventario",
                dto.InventoryItemId);
        }

        if (!item.IsActive)
        {
            throw new InventoryValidationException(
                "No se pueden registrar salidas para artículos inactivos.");
        }

        var movement = new InventoryMovement
        {
            InventoryItemId = item.Id,
            MovementType = InventoryMovementType.Salida,
            Quantity = dto.Quantity,

            // Conservamos el costo actual como referencia histórica.
            UnitCost = item.PurchaseCost,

            MovementDate = DateTime.UtcNow,
            Reason = reason,
            UserId = userId
        };

        var registered =
            await _movementRepository.RegisterAndUpdateStockAsync(
                movement,
                -dto.Quantity);

        if (registered is null)
        {
            throw new InventoryConflictException(
                "No se pudo registrar la salida. Comprueba que el artículo " +
                "esté activo y tenga existencias suficientes.");
        }

        return InventoryMapper.ToMovementResponse(registered);
    }
}