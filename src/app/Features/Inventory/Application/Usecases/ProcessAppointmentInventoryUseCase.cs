using Backend.src.app.Features.Appointments.Domain.Interfaces;
using Backend.src.app.Features.Inventory.Application.Exceptions;
using Backend.src.app.Features.Inventory.Domain.Interfaces;

namespace Backend.src.app.Features.Inventory.Application.UseCases;

public class ProcessAppointmentInventoryUseCase
    : IAppointmentInventoryProcessor
{
    private readonly IInventoryMovementRepository _movementRepository;

    public ProcessAppointmentInventoryUseCase(
        IInventoryMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task ProcessAsync(int appointmentId)
    {
        if (appointmentId <= 0)
        {
            throw new InventoryValidationException(
                "El identificador de la cita no es válido.");
        }

        var processed =
            await _movementRepository.ProcessAppointmentInventoryAsync(
                appointmentId);

        if (!processed)
        {
            throw new InventoryConflictException(
                "No se puede completar la reparación porque uno o más " +
                "artículos están inactivos o no tienen existencias suficientes.");
        }
    }
}