using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Application.Mappers;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class GetAppointmentInventoryItemsUseCase
{
    private readonly IAppointmentInventoryItemRepository _lineRepository;
    private readonly IAppointmentStatusProvider _appointmentProvider;

    public GetAppointmentInventoryItemsUseCase(
        IAppointmentInventoryItemRepository lineRepository,
        IAppointmentStatusProvider appointmentProvider)
    {
        _lineRepository = lineRepository;
        _appointmentProvider = appointmentProvider;
    }

    public async Task<List<AppointmentInventoryItemResponseDto>> Execute(
        int appointmentId,
        int userId,
        bool isAdmin)
    {
        var appointment =
            await _appointmentProvider.GetStatusAsync(appointmentId);

        if (appointment is null)
        {
            throw new InventoryNotFoundException("Cita", appointmentId);
        }

        if (!isAdmin && appointment.Value.EmployeeId != userId)
        {
            throw new InventoryForbiddenException(
                "Solo el empleado asignado puede consultar los repuestos de esta reparación.");
        }

        var lines =
            await _lineRepository.GetByAppointmentIdAsync(appointmentId);

        return InventoryMapper.ToAppointmentInventoryItemResponseList(lines);
    }
}