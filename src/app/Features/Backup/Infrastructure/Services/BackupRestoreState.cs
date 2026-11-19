namespace Backend.src.app.Features.Backup.Infrastructure.Services;

public sealed class BackupRestoreState
{
    private int _isRestoring;

    public bool IsRestoring =>
        Volatile.Read(ref _isRestoring) == 1;

    public bool TryBeginRestore()
    {
        return Interlocked.CompareExchange(
            ref _isRestoring,
            1,
            0) == 0;
    }

    public void EndRestore()
    {
        Interlocked.Exchange(ref _isRestoring, 0);
    }
}