using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class DeactivateInventoryCategoryUseCase
{
    private readonly IInventoryCategoryRepository _repository;

    public DeactivateInventoryCategoryUseCase(
        IInventoryCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task Execute(int id)
    {
        var category = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException("Categoría", id);

        if (!category.IsActive)
        {
            throw new InventoryValidationException(
                "La categoría ya está inactiva.");
        }

        if (await _repository.HasActiveItemsAsync(id))
        {
            throw new InventoryValidationException(
                "No se puede desactivar la categoría porque tiene artículos activos.");
        }

        await _repository.DeactivateAsync(id);
    }
}