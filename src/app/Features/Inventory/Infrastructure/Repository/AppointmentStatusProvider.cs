using Backend.src.app.Features.Appointments.Infrastructure.Context;
using Backend.src.app.Features.Inventory.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.src.app.Features.Inventory.Infrastructure.Repository;

public class AppointmentStatusProvider : IAppointmentStatusProvider
{
    private readonly AppointmentsDbContext _context;

    public AppointmentStatusProvider(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task<(string State, int? EmployeeId)?> GetStatusAsync(
        int appointmentId)
    {
        var appointment = await _context.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                appointment => appointment.IdPedido == appointmentId);

        if (appointment is null)
        {
            return null;
        }

        return (
            appointment.EstadoCita.ToString(),
            appointment.IdEmpleado);
    }
}