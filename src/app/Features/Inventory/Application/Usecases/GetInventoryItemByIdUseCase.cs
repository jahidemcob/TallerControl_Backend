using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetInventoryItemByIdUseCase
{
    private readonly IInventoryItemRepository _repository;

    public GetInventoryItemByIdUseCase(
        IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryItemResponseDto> Execute(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException(
                "Artículo de inventario", id);

        return InventoryMapper.ToItemResponse(item);
    }
}