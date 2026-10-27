namespace Backend.src.app.Features.Inventory.Application.Exceptions;

public class InventoryConflictException : Exception
{
    public InventoryConflictException(string message)
        : base(message)
    {
    }
}