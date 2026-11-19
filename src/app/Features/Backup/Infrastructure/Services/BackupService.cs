using System.Data;
using Backend.src.app.Features.Backup.Application.Exceptions;
using Microsoft.AspNetCore.Http;
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
    private static readonly SemaphoreSlim RestoreLock = new(1, 1);
    private readonly BackupRestoreState _restoreState;
    private readonly long _maxUploadSizeBytes;

    public BackupService(
    IConfiguration configuration,
    ILogger<BackupService> logger,
    BackupRestoreState restoreState)
    {
        _configuration = configuration;
        _logger = logger;
        _restoreState = restoreState;

        _connectionString =
            _configuration.GetConnectionString("DefaultConexion")
            ?? throw new InvalidOperationException(
                "No está configurada la conexión DefaultConexion.");

        _backupDirectory =
            _configuration["Backup:Directory"]
            ?? throw new InvalidOperationException(
                "No está configurado el directorio de backups.");

        _maxUploadSizeBytes =
            _configuration.GetValue<long?>(
                "Backup:MaxUploadSizeBytes")
            ?? 2_147_483_648L;

        if (_maxUploadSizeBytes <= 0)
        {
            throw new InvalidOperationException(
                "El tamaño máximo de backup debe ser mayor que cero.");
        }
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

    public async Task<BackupRestoreResponseDto> RestoreBackupAsync(
    IFormFile backupFile,
    CancellationToken cancellationToken = default)
    {
        if (backupFile is null || backupFile.Length == 0)
        {
            throw new BackupValidationException(
                "El archivo está vacío o no fue enviado.");
        }

        if (!string.Equals(
                Path.GetExtension(backupFile.FileName),
                ".bak",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BackupValidationException(
                "El archivo debe tener extensión .bak.");
        }

        if (backupFile.Length > _maxUploadSizeBytes)
        {
            throw new BackupValidationException(
                "El archivo supera el tamaño máximo permitido.");
        }

        // Evita ejecutar dos restauraciones simultáneamente.
        if (!await RestoreLock.WaitAsync(0, cancellationToken))
        {
            throw new BackupRestoreInProgressException();
        }

        string? uploadedBackupPath = null;

        bool backupVerified = false;
        bool maintenanceStarted = false;
        bool maintenanceCanEnd = true;

        try
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

            // Guardamos el archivo con un nombre interno seguro.
            var uploadedFileName =
                $"TallerControl_Uploaded_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.bak";

            uploadedBackupPath = Path.Combine(
                _backupDirectory,
                uploadedFileName);

            // Copiamos el archivo sin cargarlo completo en memoria.
            await using (var destination = new FileStream(
                uploadedBackupPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                options: FileOptions.Asynchronous |
                         FileOptions.SequentialScan))
            {
                await backupFile.CopyToAsync(
                    destination,
                    cancellationToken);
            }

            // Para restaurar debemos conectarnos a master,
            // no a la base de datos que vamos a reemplazar.
            var masterConnectionString =
                new SqlConnectionStringBuilder(_connectionString)
                {
                    InitialCatalog = "master"
                }.ConnectionString;

            await using var connection =
                new SqlConnection(masterConnectionString);

            await connection.OpenAsync(cancellationToken);

            // 1. Validar que el archivo pertenezca a TallerControl.
            var backupSetPosition =
                await GetAndValidateBackupSetPositionAsync(
                    connection,
                    uploadedBackupPath,
                    databaseName,
                    cancellationToken);

            // 2. Verificar la integridad del archivo.
            await VerifyBackupAsync(
                connection,
                uploadedBackupPath,
                backupSetPosition,
                cancellationToken);

            backupVerified = true;

            // 3. Crear un backup preventivo de la versión actual.
            var safetyBackupFileName =
                $"TallerControl_PreRestore_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.bak";

            var safetyBackupPath = Path.Combine(
                _backupDirectory,
                safetyBackupFileName);

            await CreateDatabaseBackupAsync(
                connection,
                databaseName,
                safetyBackupPath,
                cancellationToken);

            if (!File.Exists(safetyBackupPath) ||
                new FileInfo(safetyBackupPath).Length == 0)
            {
                throw new IOException(
                    "No se pudo crear el backup preventivo. La restauración fue cancelada.");
            }

            // 4. Activar mantenimiento del backend.
            if (!_restoreState.TryBeginRestore())
            {
                throw new BackupRestoreInProgressException();
            }

            maintenanceStarted = true;

            var escapedDatabaseName =
                EscapeSqlIdentifier(databaseName);

            bool singleUserEnabled = false;

            try
            {
                // Limpia conexiones inactivas del pool.
                SqlConnection.ClearAllPools();

                // 5. Desconectar usuarios y aplicaciones de la base.
                await ExecuteDatabaseCommandAsync(
                    connection,
                    $"""
                ALTER DATABASE [{escapedDatabaseName}]
                SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                """,
                    CancellationToken.None);

                singleUserEnabled = true;

                // 6. Restaurar la copia seleccionada.
                var restoreSql = $"""
                RESTORE DATABASE [{escapedDatabaseName}]
                FROM DISK = @BackupPath
                WITH FILE = {backupSetPosition},
                     REPLACE,
                     RECOVERY,
                     CHECKSUM,
                     STATS = 10;
                """;

                await using (var command =
                    new SqlCommand(restoreSql, connection))
                {
                    command.Parameters.Add(
                        "@BackupPath",
                        SqlDbType.NVarChar,
                        4000).Value = uploadedBackupPath;

                    // La restauración no se cancela si se pierde
                    // la conexión HTTP del administrador.
                    command.CommandTimeout = 0;

                    await command.ExecuteNonQueryAsync(
                        CancellationToken.None);
                }

                // 7. Habilitar nuevamente las conexiones.
                await ExecuteDatabaseCommandAsync(
                    connection,
                    $"""
                ALTER DATABASE [{escapedDatabaseName}]
                SET MULTI_USER;
                """,
                    CancellationToken.None);

                singleUserEnabled = false;

                SqlConnection.ClearAllPools();

                _logger.LogWarning(
                    "Base de datos {DatabaseName} restaurada desde {BackupFileName}. Backup preventivo: {SafetyBackupFileName}.",
                    databaseName,
                    uploadedFileName,
                    safetyBackupFileName);

                return new BackupRestoreResponseDto
                {
                    RestoredBackupFileName = uploadedFileName,
                    SafetyBackupFileName = safetyBackupFileName,
                    DatabaseName = databaseName,
                    RestoredAtUtc = DateTime.UtcNow
                };
            }
            finally
            {
                // Si ocurre algún error durante la restauración,
                // intentamos devolver la base al modo MULTI_USER.
                if (singleUserEnabled)
                {
                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            await connection.OpenAsync(
                                CancellationToken.None);
                        }

                        await ExecuteDatabaseCommandAsync(
                            connection,
                            $"""
                        ALTER DATABASE [{escapedDatabaseName}]
                        SET MULTI_USER WITH ROLLBACK IMMEDIATE;
                        """,
                            CancellationToken.None);
                    }
                    catch (Exception recoveryException)
                    {
                        maintenanceCanEnd = false;

                        _logger.LogCritical(
                            recoveryException,
                            "No fue posible devolver {DatabaseName} a MULTI_USER. Se mantiene bloqueada la API.",
                            databaseName);
                    }
                    finally
                    {
                        SqlConnection.ClearAllPools();
                    }
                }
            }
        }
        catch (BackupValidationException)
        {
            if (!backupVerified && uploadedBackupPath is not null)
            {
                TryDeleteInvalidUpload(uploadedBackupPath);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al restaurar el backup de TallerControl.");

            if (!backupVerified && uploadedBackupPath is not null)
            {
                TryDeleteInvalidUpload(uploadedBackupPath);
            }

            throw;
        }
        finally
        {
            if (maintenanceStarted && maintenanceCanEnd)
            {
                _restoreState.EndRestore();
            }
            else if (maintenanceStarted)
            {
                _logger.LogCritical(
                    "El backend permanece en modo mantenimiento porque no se pudo confirmar el estado MULTI_USER.");
            }

            RestoreLock.Release();
        }
    }

    private async Task<int> GetAndValidateBackupSetPositionAsync(
    SqlConnection connection,
    string backupPath,
    string expectedDatabaseName,
    CancellationToken cancellationToken)
    {
        const string sql =
            "RESTORE HEADERONLY FROM DISK = @BackupPath;";

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@BackupPath",
            SqlDbType.NVarChar,
            4000).Value = backupPath;

        command.CommandTimeout = 0;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new BackupValidationException(
                "SQL Server no encontró información de respaldo.");
        }

        var backupDatabaseName =
            reader["DatabaseName"]?.ToString();

        if (!string.Equals(
                backupDatabaseName,
                expectedDatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BackupValidationException(
                "El archivo no pertenece a la base de datos de TallerControl.");
        }

        var backupType = Convert.ToInt32(reader["BackupType"]);

        if (backupType != 1)
        {
            throw new BackupValidationException(
                "El archivo no contiene un backup completo de la base de datos.");
        }

        return Convert.ToInt32(reader["Position"]);
    }

    private static async Task VerifyBackupAsync(
    SqlConnection connection,
    string backupPath,
    int backupSetPosition,
    CancellationToken cancellationToken)
    {
        var sql = $"""
        RESTORE VERIFYONLY
        FROM DISK = @BackupPath
        WITH FILE = {backupSetPosition}, CHECKSUM;
        """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@BackupPath",
            SqlDbType.NVarChar,
            4000).Value = backupPath;

        command.CommandTimeout = 0;

        try
        {
            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }
        catch (SqlException ex)
        {
            throw new BackupValidationException(
                "SQL Server no pudo verificar la integridad del backup.",
                ex);
        }
    }

    private static async Task CreateDatabaseBackupAsync(
    SqlConnection connection,
    string databaseName,
    string backupPath,
    CancellationToken cancellationToken)
    {
        var escapedDatabaseName =
            EscapeSqlIdentifier(databaseName);

        var sql = $"""
        BACKUP DATABASE [{escapedDatabaseName}]
        TO DISK = @BackupPath
        WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10;
        """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@BackupPath",
            SqlDbType.NVarChar,
            4000).Value = backupPath;

        command.CommandTimeout = 0;

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    private static async Task ExecuteDatabaseCommandAsync(
    SqlConnection connection,
    string sql,
    CancellationToken cancellationToken)
    {
        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    private static string EscapeSqlIdentifier(
    string identifier)
    {
        return identifier.Replace(
            "]",
            "]]",
            StringComparison.Ordinal);
    }

    private void TryDeleteInvalidUpload(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo eliminar el archivo inválido {FilePath}.",
                filePath);
        }
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