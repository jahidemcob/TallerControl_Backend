namespace Backend.src.app.Features.Inventory.Application.Exceptions;

public class InventoryNotFoundException : Exception
{
    public InventoryNotFoundException(string entity, long id)
        : base($"{entity} con ID {id} no fue encontrado.")
    {
    }
}