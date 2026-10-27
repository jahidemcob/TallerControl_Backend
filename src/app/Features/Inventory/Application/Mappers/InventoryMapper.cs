using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Domain.Entities;

namespace Backend.src.app.Features.Inventory.Application.Mappers;

public static class InventoryMapper
{
    public static InventoryCategoryResponseDto ToCategoryResponse(
        InventoryCategory category)
    {
        return new InventoryCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }

    public static List<InventoryCategoryResponseDto>
        ToCategoryResponseList(
            IEnumerable<InventoryCategory> categories)
    {
        return categories
            .Select(ToCategoryResponse)
            .ToList();
    }

    public static InventoryItemResponseDto ToItemResponse(
        InventoryItem item)
    {
        return new InventoryItemResponseDto
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name ?? string.Empty,
            Code = item.Code,
            Name = item.Name,
            ItemType = item.ItemType.ToString(),
            Brand = item.Brand,
            Description = item.Description,
            AvailableQuantity = item.AvailableQuantity,
            MinimumStock = item.MinimumStock,
            IsLowStock =
                item.AvailableQuantity <= item.MinimumStock,
            PurchaseCost = item.PurchaseCost,
            SalePrice = item.SalePrice,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt
        };
    }

    public static List<InventoryItemResponseDto>
        ToItemResponseList(
            IEnumerable<InventoryItem> items)
    {
        return items
            .Select(ToItemResponse)
            .ToList();
    }

    public static InventoryMovementResponseDto ToMovementResponse(
    InventoryMovement movement)
    {
        return new InventoryMovementResponseDto
        {
            Id = movement.Id,
            InventoryItemId = movement.InventoryItemId,
            ItemCode = movement.InventoryItem?.Code ?? string.Empty,
            ItemName = movement.InventoryItem?.Name ?? string.Empty,
            MovementType = movement.MovementType.ToString(),
            Quantity = movement.Quantity,
            UnitCost = movement.UnitCost,
            MovementDate = movement.MovementDate,
            Reason = movement.Reason,
            AppointmentId = movement.AppointmentId,
            UserId = movement.UserId,
            AppointmentInventoryItemId =
                movement.AppointmentInventoryItemId
        };
    }

    public static List<InventoryMovementResponseDto>
    ToMovementResponseList(
        IEnumerable<InventoryMovement> movements)
    {
        return movements
            .Select(ToMovementResponse)
            .ToList();
    }

    public static AppointmentInventoryItemResponseDto
    ToAppointmentInventoryItemResponse(
        AppointmentInventoryItem line)
    {
        return new AppointmentInventoryItemResponseDto
        {
            Id = line.Id,
            AppointmentId = line.AppointmentId,
            InventoryItemId = line.InventoryItemId,
            ItemCode = line.InventoryItem?.Code ?? string.Empty,
            ItemName = line.InventoryItem?.Name ?? string.Empty,
            Quantity = line.Quantity,
            UnitCost = line.UnitCost,
            UnitPrice = line.UnitPrice,
            Subtotal = line.Subtotal,
            UserId = line.UserId,
            RecordedAt = line.RecordedAt,
            InventoryProcessed = line.InventoryProcessed,
            InventoryProcessedAt = line.InventoryProcessedAt
        };
    }

    public static List<AppointmentInventoryItemResponseDto>
        ToAppointmentInventoryItemResponseList(
            IEnumerable<AppointmentInventoryItem> lines)
    {
        return lines
            .Select(ToAppointmentInventoryItemResponse)
            .ToList();
    }
}