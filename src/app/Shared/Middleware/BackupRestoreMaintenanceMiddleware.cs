using Backend.src.app.Features.Backup.Infrastructure.Services;

namespace Backend.src.app.Shared.Middleware;

public class BackupRestoreMaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BackupRestoreState _restoreState;

    public BackupRestoreMaintenanceMiddleware(
        RequestDelegate next,
        BackupRestoreState restoreState)
    {
        _next = next;
        _restoreState = restoreState;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isRestoreRequest =
            HttpMethods.IsPost(context.Request.Method) &&
            string.Equals(
                context.Request.Path.Value,
                "/api/backup/restore",
                StringComparison.OrdinalIgnoreCase);

        var isHealthRequest =
            context.Request.Path.StartsWithSegments("/health");

        // No bloqueamos el endpoint de restauración ni el healthcheck.
        if (!_restoreState.IsRestoring ||
            isRestoreRequest ||
            isHealthRequest ||
            !context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode =
            StatusCodes.Status503ServiceUnavailable;

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status503ServiceUnavailable,
            error = "DatabaseMaintenance",
            message = "TallerControl está restaurando una copia de seguridad. Intente nuevamente cuando finalice el proceso."
        });
    }
}