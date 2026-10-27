using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class CreateInventoryItemUseCase
{
    private readonly IInventoryItemRepository _itemRepository;
    private readonly IInventoryCategoryRepository _categoryRepository;

    public CreateInventoryItemUseCase(
        IInventoryItemRepository itemRepository,
        IInventoryCategoryRepository categoryRepository)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<InventoryItemResponseDto> Execute(
        InventoryItemRequestDto dto)
    {
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
                "No se pueden registrar artículos en una categoría inactiva.");
        }

        if (await _itemRepository.ExistsByCodeAsync(code))
        {
            throw new InventoryConflictException(
                $"Ya existe un artículo con el código '{code}'.");
        }

        var item = new InventoryItem
        {
            CategoryId = dto.CategoryId,
            Code = code,
            Name = name,
            ItemType = itemType,
            Brand = NormalizeText(dto.Brand),
            Description = NormalizeText(dto.Description),
            AvailableQuantity = 0m,
            MinimumStock = dto.MinimumStock,
            PurchaseCost = dto.PurchaseCost,
            SalePrice = dto.SalePrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _itemRepository.CreateAsync(item);

        // Consultamos de nuevo para recuperar el nombre de la categoría.
        var result = await _itemRepository.GetByIdAsync(created.Id)
            ?? throw new InvalidOperationException(
                "El artículo fue creado, pero no pudo recuperarse.");

        return InventoryMapper.ToItemResponse(result);
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}