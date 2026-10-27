using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetInventoryCategoriesUseCase
{
    private readonly IInventoryCategoryRepository _repository;

    public GetInventoryCategoriesUseCase(
        IInventoryCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<InventoryCategoryResponseDto>> Execute(
        bool includeInactive = false)
    {
        var categories = await _repository.GetAllAsync(includeInactive);

        return InventoryMapper.ToCategoryResponseList(categories);
    }
}