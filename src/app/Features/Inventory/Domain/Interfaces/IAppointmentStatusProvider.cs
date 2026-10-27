namespace Backend.src.app.Features.Inventory.Domain.Interfaces;

public interface IAppointmentStatusProvider
{
    Task<(string State, int? EmployeeId)?> GetStatusAsync(
        int appointmentId);
}