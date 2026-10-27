using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class CreateInventoryEntryUseCase
{
    private readonly IInventoryItemRepository _itemRepository;
    private readonly IInventoryMovementRepository _movementRepository;

    public CreateInventoryEntryUseCase(
        IInventoryItemRepository itemRepository,
        IInventoryMovementRepository movementRepository)
    {
        _itemRepository = itemRepository;
        _movementRepository = movementRepository;
    }

    public async Task<InventoryMovementResponseDto> Execute(
        int userId,
        InventoryEntryRequestDto dto)
    {
        if (userId <= 0)
        {
            throw new InventoryValidationException(
                "El usuario responsable no es válido.");
        }

        if (dto.Quantity <= 0 || dto.UnitCost < 0)
        {
            throw new InventoryValidationException(
                "La cantidad debe ser positiva y el costo no puede ser negativo.");
        }

        var reason = dto.Reason.Trim();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InventoryValidationException(
                "El motivo de la entrada es obligatorio.");
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
                "No se pueden registrar entradas para artículos inactivos.");
        }

        var movement = new InventoryMovement
        {
            InventoryItemId = dto.InventoryItemId,
            MovementType = InventoryMovementType.Entrada,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            MovementDate = DateTime.UtcNow,
            Reason = reason,
            UserId = userId
        };

        var registered =
            await _movementRepository.RegisterAndUpdateStockAsync(
                movement,
                dto.Quantity,
                dto.UnitCost);

        if (registered is null)
        {
            throw new InventoryConflictException(
                "No fue posible registrar la entrada. Verifica el estado " +
                "y las existencias del artículo.");
        }

        return InventoryMapper.ToMovementResponse(registered);
    }
}