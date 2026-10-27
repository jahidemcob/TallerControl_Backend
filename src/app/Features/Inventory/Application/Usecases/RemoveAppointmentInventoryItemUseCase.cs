using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class RemoveAppointmentInventoryItemUseCase
{
    private readonly IAppointmentInventoryItemRepository _lineRepository;
    private readonly IAppointmentStatusProvider _appointmentProvider;

    public RemoveAppointmentInventoryItemUseCase(
        IAppointmentInventoryItemRepository lineRepository,
        IAppointmentStatusProvider appointmentProvider)
    {
        _lineRepository = lineRepository;
        _appointmentProvider = appointmentProvider;
    }

    public async Task Execute(
        int appointmentId,
        long id,
        int userId,
        bool isAdmin)
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
                "Solo puedes retirar artículos de una cita que esté en proceso.");
        }

        if (!isAdmin && appointment.Value.EmployeeId != userId)
        {
            throw new InventoryForbiddenException(
                "Solo el empleado asignado puede modificar estos consumos.");
        }

        if (await _lineRepository.HasProcessedItemsAsync(appointmentId))
        {
            throw new InventoryConflictException(
                "La reparación ya tiene consumos procesados.");
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
                "No puedes eliminar un consumo que ya fue procesado.");
        }

        var deleted = await _lineRepository.DeletePendingAsync(
            appointmentId, id);

        if (!deleted)
        {
            throw new InventoryConflictException(
                "No fue posible retirar el artículo de la reparación.");
        }
    }
}