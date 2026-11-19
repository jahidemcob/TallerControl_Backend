namespace Backend.src.app.Features.Backup.Application.Exceptions;

public class BackupRestoreInProgressException : Exception
{
    public BackupRestoreInProgressException()
        : base("Ya hay una restauración de backup en curso.")
    {
    }
}