using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class UpdateInventoryItemUseCase
{
    private readonly IInventoryItemRepository _itemRepository;
    private readonly IInventoryCategoryRepository _categoryRepository;

    public UpdateInventoryItemUseCase(
        IInventoryItemRepository itemRepository,
        IInventoryCategoryRepository categoryRepository)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<InventoryItemResponseDto> Execute(
        int id,
        InventoryItemRequestDto dto)
    {
        var current = await _itemRepository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException(
                "Artículo de inventario", id);

        if (!current.IsActive)
        {
            throw new InventoryValidationException(
                "No se puede editar un artículo inactivo.");
        }

        var code = dto.Code.Trim();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(name))
        {
            throw new InventoryValidationException(
                "El código y el nombre son obligatorios.");
        }

        if (!Enum.TryParse<InventoryItemType>(
                dto.ItemType.Trim(),
                true,
                out var itemType) ||
            !Enum.IsDefined(typeof(InventoryItemType), itemType))
        {
            throw new InventoryValidationException(
                "El tipo debe ser Repuesto o Consumible.");
        }

        if (dto.MinimumStock < 0 ||
            dto.PurchaseCost < 0 ||
            dto.SalePrice < 0)
        {
            throw new InventoryValidationException(
                "El stock mínimo y los precios no pueden ser negativos.");
        }

        var category = await _categoryRepository.GetByIdAsync(
            dto.CategoryId);

        if (category is null)
        {
            throw new InventoryNotFoundException(
                "Categoría", dto.CategoryId);
        }

        if (!category.IsActive)
        {
            throw new InventoryValidationException(
                "No se puede asignar un artículo a una categoría inactiva.");
        }

        if (await _itemRepository.ExistsByCodeAsync(code, id))
        {
            throw new InventoryConflictException(
                $"Ya existe otro artículo con el código '{code}'.");
        }

        var updatedItem = new InventoryItem
        {
            Id = id,
            CategoryId = dto.CategoryId,
            Code = code,
            Name = name,
            ItemType = itemType,
            Brand = NormalizeText(dto.Brand),
            Description = NormalizeText(dto.Description),
            MinimumStock = dto.MinimumStock,
            PurchaseCost = dto.PurchaseCost,
            SalePrice = dto.SalePrice
        };

        await _itemRepository.UpdateAsync(id, updatedItem);

        var result = await _itemRepository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException(
                "Artículo de inventario", id);

        return InventoryMapper.ToItemResponse(result);
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}