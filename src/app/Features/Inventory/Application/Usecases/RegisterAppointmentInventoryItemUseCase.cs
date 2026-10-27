using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Entities;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class RegisterAppointmentInventoryItemUseCase
{
    private readonly IInventoryItemRepository _itemRepository;
    private readonly IAppointmentInventoryItemRepository _lineRepository;
    private readonly IAppointmentStatusProvider _appointmentProvider;

    public RegisterAppointmentInventoryItemUseCase(
        IInventoryItemRepository itemRepository,
        IAppointmentInventoryItemRepository lineRepository,
        IAppointmentStatusProvider appointmentProvider)
    {
        _itemRepository = itemRepository;
        _lineRepository = lineRepository;
        _appointmentProvider = appointmentProvider;
    }

    public async Task<AppointmentInventoryItemResponseDto> Execute(
        int appointmentId,
        int userId,
        bool isAdmin,
        AppointmentInventoryItemRequestDto dto)
    {
        if (appointmentId <= 0 || userId <= 0 || dto.Quantity <= 0)
        {
            throw new InventoryValidationException(
                "La cita, el usuario y la cantidad deben ser válidos.");
        }

        var appointment =
            await _appointmentProvider.GetStatusAsync(appointmentId);

        if (appointment is null)
        {
            throw new InventoryNotFoundException("Cita", appointmentId);
        }

        if (appointment.Value.State != "EnProceso")
        {
            throw new InventoryConflictException(
                "Solo puedes registrar repuestos en una cita que esté en proceso.");
        }

        if (!isAdmin && appointment.Value.EmployeeId != userId)
        {
            throw new InventoryForbiddenException(
                "Solo el empleado asignado puede registrar los repuestos utilizados.");
        }

        if (await _lineRepository.HasProcessedItemsAsync(appointmentId))
        {
            throw new InventoryConflictException(
                "La reparación ya tiene consumos procesados. No se pueden agregar nuevos artículos.");
        }

        var item = await _itemRepository.GetByIdAsync(dto.InventoryItemId);

        if (item is null)
        {
            throw new InventoryNotFoundException(
                "Artículo de inventario", dto.InventoryItemId);
        }

        if (!item.IsActive)
        {
            throw new InventoryValidationException(
                "No se pueden utilizar artículos inactivos.");
        }

        if (item.Category is null || !item.Category.IsActive)
        {
            throw new InventoryValidationException(
                "El artículo pertenece a una categoría inactiva.");
        }

        if (await _lineRepository.ExistsByAppointmentAndItemAsync(
                appointmentId, dto.InventoryItemId))
        {
            throw new InventoryConflictException(
                "Este artículo ya está registrado en la reparación. " +
                "Edita su cantidad en lugar de registrarlo otra vez.");
        }

        var line = new AppointmentInventoryItem
        {
            AppointmentId = appointmentId,
            InventoryItemId = item.Id,
            Quantity = dto.Quantity,
            UnitCost = item.PurchaseCost,
            UnitPrice = item.SalePrice,
            UserId = userId,
            RecordedAt = DateTime.UtcNow,
            InventoryProcessed = false,
            InventoryProcessedAt = null
        };

        var created = await _lineRepository.CreateAsync(line);

        return InventoryMapper.ToAppointmentInventoryItemResponse(created);
    }
}