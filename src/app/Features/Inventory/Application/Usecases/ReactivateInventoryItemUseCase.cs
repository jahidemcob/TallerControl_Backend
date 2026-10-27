using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class ReactivateInventoryItemUseCase
{
    private readonly IInventoryItemRepository _itemRepository;

    public ReactivateInventoryItemUseCase(
        IInventoryItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task Execute(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException(
                "Artículo de inventario", id);

        if (item.IsActive)
        {
            throw new InventoryValidationException(
                "El artículo ya está activo.");
        }

        if (item.Category is null || !item.Category.IsActive)
        {
            throw new InventoryValidationException(
                "No se puede reactivar el artículo porque su categoría está inactiva. " +
                "Reactiva primero la categoría.");
        }

        var activated = await _itemRepository.ActivateAsync(id);

        if (!activated)
        {
            throw new InventoryNotFoundException(
                "Artículo de inventario", id);
        }
    }
}