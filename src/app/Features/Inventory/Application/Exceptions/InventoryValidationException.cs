namespace Backend.src.app.Features.Inventory.Application.Exceptions;

public class InventoryValidationException : Exception
{
    public InventoryValidationException(string message)
        : base(message)
    {
    }
}