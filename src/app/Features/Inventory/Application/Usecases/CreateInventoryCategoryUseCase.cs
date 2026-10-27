using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class CreateInventoryCategoryUseCase
{
    private readonly IInventoryCategoryRepository _repository;

    public CreateInventoryCategoryUseCase(
        IInventoryCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryCategoryResponseDto> Execute(
        InventoryCategoryRequestDto dto)
    {
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InventoryValidationException(
                "El nombre de la categoría es obligatorio.");
        }

        if (await _repository.ExistsByNameAsync(name))
        {
            throw new InventoryConflictException(
                $"Ya existe una categoría con el nombre '{name}'.");
        }

        var category = new InventoryCategory
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(category);

        return InventoryMapper.ToCategoryResponse(created);
    }
}