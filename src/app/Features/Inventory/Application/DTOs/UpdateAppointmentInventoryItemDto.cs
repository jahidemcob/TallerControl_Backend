using System.ComponentModel.DataAnnotations;

namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class UpdateAppointmentInventoryItemDto
{
    [Range(0.01, 99999999.99)]
    public decimal Quantity { get; set; }
}