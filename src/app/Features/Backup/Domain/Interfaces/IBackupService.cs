using Backend.src.app.Features.Backup.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Backend.src.app.Features.Backup.Domain.Interfaces;

public interface IBackupService
{
    Task<BackupResponseDto> CreateBackupAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackupResponseDto>> GetBackupsAsync(
        CancellationToken cancellationToken = default);

    Task<string> GetBackupPathAsync(
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteBackupAsync(
        string fileName,
        CancellationToken cancellationToken = default);

    Task<BackupRestoreResponseDto> RestoreBackupAsync(
        IFormFile backupFile,
        CancellationToken cancellationToken = default);
}