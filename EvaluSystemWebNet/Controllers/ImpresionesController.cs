using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImpresionesController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public ImpresionesController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ImpresionArchivoDto>>> GetAll(
        [FromQuery] string? fechaDesde,
        [FromQuery] string? fechaHasta,
        [FromQuery] int? maquinaId,
        CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(fechaDesde))
        {
            query.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde)}");
        }

        if (!string.IsNullOrWhiteSpace(fechaHasta))
        {
            query.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta)}");
        }

        if (maquinaId.HasValue)
        {
            query.Add($"maquinaId={maquinaId.Value}");
        }

        var endpoint = "api/Impresiones" + (query.Count > 0 ? $"?{string.Join("&", query)}" : string.Empty);
        var result = await _backendApiClient.GetResultAsync<IEnumerable<ImpresionArchivoDto>>(endpoint, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    [HttpGet("{detalleId:int}/archivo")]
    public async Task<IActionResult> DescargarArchivo(int detalleId, CancellationToken cancellationToken)
    {
        var file = await _backendApiClient.GetAsync<ExcelFileDto>($"api/Impresiones/{detalleId}/archivo", cancellationToken);
        if (file is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo descargar el archivo." });
        }

        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }
}
