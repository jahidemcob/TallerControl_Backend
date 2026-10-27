using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetInventoryMovementByIdUseCase
{
    private readonly IInventoryMovementRepository _repository;

    public GetInventoryMovementByIdUseCase(
        IInventoryMovementRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryMovementResponseDto> Execute(long id)
    {
        var movement = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException(
                "Movimiento de inventario", id);

        return InventoryMapper.ToMovementResponse(movement);
    }
}