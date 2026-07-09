using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/grupo-ventas")]
public class GrupoVentasController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public GrupoVentasController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("mi-equipo")]
    public async Task<IActionResult> GetMiEquipo(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>();
        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        var result = await _backendApiClient.GetResultAsync<GrupoVentaEquipoDto>(
            $"api/GruposVenta/mi-equipo?{string.Join("&", filters)}",
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    [HttpGet("mi-equipo/excel")]
    public async Task<IActionResult> ExportMiEquipoExcel(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>();
        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        var result = await _backendApiClient.GetResultAsync<ExcelFileDto>(
            $"api/GruposVenta/mi-equipo/excel?{string.Join("&", filters)}",
            cancellationToken);

        if (!result.IsSuccess || result.Value is null || string.IsNullOrWhiteSpace(result.Value.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo exportar grupo de ventas." });
        }

        var bytes = Convert.FromBase64String(result.Value.Bytes);
        return File(bytes, result.Value.ContentType, result.Value.FileName);
    }
}
