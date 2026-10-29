using Backend.src.app.Features.Backup.Application.DTOs;
using Backend.src.app.Features.Backup.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Backend.src.app.Features.Backup.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupDirectory;
    private readonly string _connectionString;

    public BackupService(
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _connectionString =
            _configuration.GetConnectionString("DefaultConexion")
            ?? throw new InvalidOperationException(
                "No está configurada la conexión DefaultConexion.");

        _backupDirectory =
            _configuration["Backup:Directory"]
            ?? throw new InvalidOperationException(
                "No está configurado el directorio de backups.");
    }

    public async Task<BackupResponseDto> CreateBackupAsync(
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_backupDirectory);

        var connectionBuilder =
            new SqlConnectionStringBuilder(_connectionString);

        var databaseName = connectionBuilder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException(
                "La conexión no tiene una base de datos configurada.");
        }

        var fileName =
            $"TallerControl_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.bak";

        var filePath = Path.Combine(
            _backupDirectory,
            fileName);

        var escapedDatabaseName =
            databaseName.Replace("]", "]]");

        try
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            var sql = $"""
                BACKUP DATABASE [{escapedDatabaseName}]
                TO DISK = @BackupPath
                WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10;
                """;

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@BackupPath",
                filePath);

            // El backup puede tardar más que una consulta normal.
            command.CommandTimeout = 0;

            await command.ExecuteNonQueryAsync(
                cancellationToken);

            if (!File.Exists(filePath))
            {
                throw new IOException(
                    "SQL Server finalizó la operación, pero no se encontró el archivo.");
            }

            var fileInfo = new FileInfo(filePath);

            _logger.LogInformation(
                "Backup creado correctamente: {FileName}",
                fileName);

            return new BackupResponseDto
            {
                FileName = fileInfo.Name,
                SizeInBytes = fileInfo.Length,
                CreatedAt = fileInfo.LastWriteTimeUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al crear el backup de la base de datos {DatabaseName}.",
                databaseName);

            throw;
        }
    }

    public Task<IReadOnlyList<BackupResponseDto>> GetBackupsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_backupDirectory);

        IReadOnlyList<BackupResponseDto> backups =
            Directory.GetFiles(
                    _backupDirectory,
                    "*.bak",
                    SearchOption.TopDirectoryOnly)
                .Select(path =>
                {
                    var info = new FileInfo(path);

                    return new BackupResponseDto
                    {
                        FileName = info.Name,
                        SizeInBytes = info.Length,
                        CreatedAt = info.LastWriteTimeUtc
                    };
                })
                .OrderByDescending(backup => backup.CreatedAt)
                .ToList();

        return Task.FromResult(backups);
    }

    public Task<string> GetBackupPathAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsValidFileName(fileName))
        {
            throw new ArgumentException(
                "El nombre del archivo no es válido.",
                nameof(fileName));
        }

        var filePath = Path.Combine(
            _backupDirectory,
            fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "No se encontró el backup solicitado.");
        }

        return Task.FromResult(filePath);
    }

    public Task DeleteBackupAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsValidFileName(fileName))
        {
            throw new ArgumentException(
                "El nombre del archivo no es válido.",
                nameof(fileName));
        }

        var filePath = Path.Combine(
            _backupDirectory,
            fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "No se encontró el backup solicitado.");
        }

        File.Delete(filePath);

        _logger.LogWarning(
            "Se eliminó el backup {FileName}.",
            fileName);

        return Task.CompletedTask;
    }

    private static bool IsValidFileName(string fileName)
    {
        return !string.IsNullOrWhiteSpace(fileName)
            && string.Equals(
                fileName,
                Path.GetFileName(fileName),
                StringComparison.Ordinal)
            && fileName.EndsWith(
                ".bak",
                StringComparison.OrdinalIgnoreCase)
            && fileName.StartsWith(
                "TallerControl_",
                StringComparison.OrdinalIgnoreCase);
    }
}