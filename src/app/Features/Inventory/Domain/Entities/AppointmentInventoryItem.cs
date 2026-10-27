namespace Backend.src.app.Features.Inventory.Domain.Entities;

public class AppointmentInventoryItem
{
    public long Id { get; set; }

    // Identificador de Pedidos.idPedido.
    public int AppointmentId { get; set; }

    public int InventoryItemId { get; set; }

    public decimal Quantity { get; set; }

    // Precios congelados al registrar el consumo.
    public decimal UnitCost { get; set; }

    public decimal UnitPrice { get; set; }

    // SQL Server calculará Quantity * UnitPrice.
    public decimal Subtotal { get; private set; }

    // Usuario que registró el artículo utilizado.
    public int UserId { get; set; }

    public DateTime RecordedAt { get; set; }

    // Evita procesar dos veces el mismo consumo.
    public bool InventoryProcessed { get; set; }

    public DateTime? InventoryProcessedAt { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public ICollection<InventoryMovement> Movements { get; set; }
        = new List<InventoryMovement>();
}