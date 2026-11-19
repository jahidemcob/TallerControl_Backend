using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend.src.app.Features.Backup.Application.DTOs;

public class BackupRestoreRequestDto
{
    [Required(ErrorMessage = "Debe seleccionar un archivo de respaldo.")]
    public IFormFile? BackupFile { get; set; }
}