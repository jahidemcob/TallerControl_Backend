using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetInventoryCategoryByIdUseCase
{
    private readonly IInventoryCategoryRepository _repository;

    public GetInventoryCategoryByIdUseCase(
        IInventoryCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryCategoryResponseDto> Execute(int id)
    {
        var category = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException("Categoría", id);

        return InventoryMapper.ToCategoryResponse(category);
    }
}