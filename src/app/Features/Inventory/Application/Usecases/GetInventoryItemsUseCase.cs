using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetInventoryItemsUseCase
{
    private readonly IInventoryItemRepository _repository;

    public GetInventoryItemsUseCase(
        IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<InventoryItemResponseDto>> Execute(
        int? categoryId = null,
        InventoryItemType? itemType = null,
        bool includeInactive = false,
        bool onlyLowStock = false,
        string? search = null)
    {
        var items = await _repository.GetAllAsync(
            categoryId,
            itemType,
            includeInactive,
            onlyLowStock,
            search);

        return InventoryMapper.ToItemResponseList(items);
    }
}