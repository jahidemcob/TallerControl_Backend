namespace Backend.src.app.Features.Backup.Application.DTOs;

public class BackupRestoreResponseDto
{
    public string RestoredBackupFileName { get; set; } = string.Empty;

    public string SafetyBackupFileName { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public DateTime RestoredAtUtc { get; set; }
}