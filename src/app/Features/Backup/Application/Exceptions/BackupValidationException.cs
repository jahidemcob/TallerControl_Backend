namespace Backend.src.app.Features.Backup.Application.Exceptions;

public class BackupValidationException : Exception
{
    public BackupValidationException(string message)
        : base(message)
    {
    }

    public BackupValidationException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}