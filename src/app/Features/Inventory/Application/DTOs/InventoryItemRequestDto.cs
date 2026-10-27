using System.ComponentModel.DataAnnotations;

namespace Backend.src.app.Features.Inventory.Application.DTOs;

public class InventoryItemRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de artículo es obligatorio.")]
    public string ItemType { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.0, 99999999.99,
    ErrorMessage = "El stock mínimo debe ser mayor o igual a cero.")]
    public decimal MinimumStock { get; set; }

    [Range(0.0, 9999999999.99,
        ErrorMessage = "El costo de compra debe ser mayor o igual a cero.")]
    public decimal PurchaseCost { get; set; }

    [Range(0.0, 9999999999.99,
        ErrorMessage = "El precio de venta debe ser mayor o igual a cero.")]
    public decimal SalePrice { get; set; }
}