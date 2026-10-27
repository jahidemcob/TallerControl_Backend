using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetInventoryMovementsUseCase
{
    private readonly IInventoryMovementRepository _repository;

    public GetInventoryMovementsUseCase(
        IInventoryMovementRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<InventoryMovementResponseDto>> Execute(
        int? inventoryItemId = null,
        InventoryMovementType? movementType = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        if (from.HasValue &&
            to.HasValue &&
            from.Value > to.Value)
        {
            throw new InventoryValidationException(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }

        var movements = await _repository.GetAllAsync(
            inventoryItemId,
            movementType,
            from,
            to);

        return InventoryMapper.ToMovementResponseList(movements);
    }
}