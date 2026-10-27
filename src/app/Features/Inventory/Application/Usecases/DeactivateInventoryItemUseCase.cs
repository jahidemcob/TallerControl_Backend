using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class DeactivateInventoryItemUseCase
{
    private readonly IInventoryItemRepository _repository;

    public DeactivateInventoryItemUseCase(
        IInventoryItemRepository repository)
    {
        _repository = repository;
    }

    public async Task Execute(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException(
                "Artículo de inventario", id);

        if (!item.IsActive)
        {
            throw new InventoryValidationException(
                "El artículo ya está inactivo.");
        }

        if (item.AvailableQuantity > 0)
        {
            throw new InventoryConflictException(
                "No se puede desactivar el artículo porque todavía tiene existencias.");
        }

        await _repository.DeactivateAsync(id);
    }
}