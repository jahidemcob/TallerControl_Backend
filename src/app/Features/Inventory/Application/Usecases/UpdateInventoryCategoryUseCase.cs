using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class UpdateInventoryCategoryUseCase
{
    private readonly IInventoryCategoryRepository _repository;

    public UpdateInventoryCategoryUseCase(
        IInventoryCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryCategoryResponseDto> Execute(
        int id,
        InventoryCategoryRequestDto dto)
    {
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InventoryValidationException(
                "El nombre de la categoría es obligatorio.");
        }

        var current = await _repository.GetByIdAsync(id)
            ?? throw new InventoryNotFoundException("Categoría", id);

        if (!current.IsActive)
        {
            throw new InventoryValidationException(
                "No se puede editar una categoría inactiva.");
        }

        if (await _repository.ExistsByNameAsync(name, id))
        {
            throw new InventoryConflictException(
                $"Ya existe una categoría con el nombre '{name}'.");
        }

        var updatedCategory = new InventoryCategory
        {
            Id = id,
            Name = name,
            Description = dto.Description,
            IsActive = current.IsActive,
            CreatedAt = current.CreatedAt
        };

        var updated = await _repository.UpdateAsync(id, updatedCategory)
            ?? throw new InventoryNotFoundException("Categoría", id);

        return InventoryMapper.ToCategoryResponse(updated);
    }
}