using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class ReactivateInventoryCategoryUseCase
{
    private readonly IInventoryCategoryRepository _repository;

    public ReactivateInventoryCategoryUseCase(
        IInventoryCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task Execute(int id)
    {
        var category = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException("Categoría", id);

        if (category.IsActive)
        {
            throw new InventoryValidationException(
                "La categoría ya está activa.");
        }

        await _repository.ActivateAsync(id);
    }
}