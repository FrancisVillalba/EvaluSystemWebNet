using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchivosController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public ArchivosController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(IFormFile archivo, [FromForm] string? carpeta = null, CancellationToken cancellationToken = default)
    {
        if (archivo.Length == 0)
        {
            return BadRequest(new { message = "El archivo esta vacio." });
        }

        var result = await _backendApiClient.PostFileResultAsync<ArchivoUploadResponse>(
            "api/Archivos/upload",
            archivo,
            new Dictionary<string, string?> { ["carpeta"] = carpeta },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string ruta, [FromQuery] string? nombreDescarga = null, CancellationToken cancellationToken = default)
    {
        var query = $"ruta={Uri.EscapeDataString(ruta)}";
        if (!string.IsNullOrWhiteSpace(nombreDescarga))
        {
            query += $"&nombreDescarga={Uri.EscapeDataString(nombreDescarga)}";
        }

        var file = await _backendApiClient.GetAsync<ArchivoBase64Response>($"api/Archivos/base64?{query}", cancellationToken);
        if (file is null || string.IsNullOrWhiteSpace(file.Base64))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo descargar el archivo desde EvaluSystemBack." });
        }

        return File(Convert.FromBase64String(file.Base64), file.ContentType, file.NombreArchivo);
    }
}
