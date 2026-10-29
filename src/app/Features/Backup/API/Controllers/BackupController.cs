using Backend.src.app.Features.Backup.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.src.app.Features.Backup.API.Controllers;

[ApiController]
[Route("api/backup")]
[Authorize(Roles = "Administrador")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    // POST: api/backup
    [HttpPost]
    public async Task<IActionResult> CreateBackup(
        CancellationToken cancellationToken)
    {
        var backup = await _backupService.CreateBackupAsync(
            cancellationToken);

        return Ok(new
        {
            message = "Backup creado correctamente.",
            data = backup
        });
    }

    // GET: api/backup
    [HttpGet]
    public async Task<IActionResult> GetBackups(
        CancellationToken cancellationToken)
    {
        var backups = await _backupService.GetBackupsAsync(
            cancellationToken);

        return Ok(backups);
    }

    // GET: api/backup/download/{fileName}
    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadBackup(
        string fileName,
        CancellationToken cancellationToken)
    {
        var filePath = await _backupService.GetBackupPathAsync(
            fileName,
            cancellationToken);

        return PhysicalFile(
            filePath,
            "application/octet-stream",
            Path.GetFileName(filePath));
    }

    // DELETE: api/backup/{fileName}
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteBackup(
        string fileName,
        CancellationToken cancellationToken)
    {
        await _backupService.DeleteBackupAsync(
            fileName,
            cancellationToken);

        return Ok(new
        {
            message = "Backup eliminado correctamente."
        });
    }
}