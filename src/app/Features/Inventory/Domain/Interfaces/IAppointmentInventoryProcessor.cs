namespace Backend.src.app.Features.Appointments.Domain.Interfaces;

public interface IAppointmentInventoryProcessor
{
    Task ProcessAsync(int appointmentId);
}