using System.ComponentModel.DataAnnotations;

namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class InventoryExitRequestDto
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Debes seleccionar un artículo válido.")]
    public int InventoryItemId { get; set; }

    [Range(0.01, 99999999.99,
        ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "El motivo de la salida es obligatorio.")]
    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}