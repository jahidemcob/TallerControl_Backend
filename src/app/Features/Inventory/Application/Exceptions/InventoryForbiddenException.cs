namespace Backend.src.app.Features.Inventory.Application.Exceptions;

public class InventoryForbiddenException : Exception
{
    public InventoryForbiddenException(string message)
        : base(message)
    {
    }
}