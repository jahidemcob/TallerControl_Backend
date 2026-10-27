using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class UpdateAppointmentInventoryItemUseCase
{
    private readonly IAppointmentInventoryItemRepository _lineRepository;
    private readonly IAppointmentStatusProvider _appointmentProvider;

    public UpdateAppointmentInventoryItemUseCase(
        IAppointmentInventoryItemRepository lineRepository,
        IAppointmentStatusProvider appointmentProvider)
    {
        _lineRepository = lineRepository;
        _appointmentProvider = appointmentProvider;
    }

    public async Task<AppointmentInventoryItemResponseDto> Execute(
        int appointmentId,
        long id,
        int userId,
        bool isAdmin,
        UpdateAppointmentInventoryItemDto dto)
    {
        var appointment =
            await _appointmentProvider.GetStatusAsync(appointmentId);

        if (appointment is null)
        {
            throw new InventoryNotFoundException("Cita", appointmentId);
        }

        if (appointment.Value.State != "EnProceso")
        {
            throw new InventoryConflictException(
                "Solo puedes modificar consumos de una cita que esté en proceso.");
        }

        if (!isAdmin && appointment.Value.EmployeeId != userId)
        {
            throw new InventoryForbiddenException(
                "Solo el empleado asignado puede modificar estos consumos.");
        }

        if (dto.Quantity <= 0)
        {
            throw new InventoryValidationException(
                "La cantidad debe ser mayor que cero.");
        }

        if (await _lineRepository.HasProcessedItemsAsync(appointmentId))
        {
            throw new InventoryConflictException(
                "Ya hay consumos procesados para esta reparación.");
        }

        var current = await _lineRepository.GetByIdAsync(id);

        if (current is null || current.AppointmentId != appointmentId)
        {
            throw new InventoryNotFoundException(
                "Artículo utilizado en la reparación", id);
        }

        if (current.InventoryProcessed)
        {
            throw new InventoryConflictException(
                "No se puede modificar un consumo ya procesado.");
        }

        var updated = await _lineRepository.UpdateQuantityAsync(
            appointmentId, id, dto.Quantity);

        if (updated is null)
        {
            throw new InventoryConflictException(
                "No fue posible modificar el consumo.");
        }

        return InventoryMapper.ToAppointmentInventoryItemResponse(updated);
    }
}