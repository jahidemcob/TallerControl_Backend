namespace Backend.src.app.Features.Backup.Application.DTOs;

public class BackupResponseDto
{
    public string FileName { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    public DateTime CreatedAt { get; set; }
}