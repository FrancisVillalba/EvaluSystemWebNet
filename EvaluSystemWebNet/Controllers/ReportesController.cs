using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public ReportesController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<object>> GetOptions(CancellationToken cancellationToken)
    {
        var usuarios = await _backendApiClient.GetAsync<IEnumerable<UsuarioDto>>("api/Usuarios", cancellationToken);
        if (usuarios is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudieron obtener opciones de reportes." });
        }

        var gruposVenta = await _backendApiClient.GetAsync<IEnumerable<GrupoVentaDto>>("api/GruposVenta", cancellationToken)
            ?? Enumerable.Empty<GrupoVentaDto>();
        var vendedoresExternosIds = gruposVenta
            .Where(grupo => grupo.Estado)
            .SelectMany(grupo => grupo.Vendedores.Where(vendedor => vendedor.Estado))
            .Select(vendedor => vendedor.VendedorUsuarioId)
            .ToHashSet();

        var vendedores = usuarios
            .Where(IsPerfilVendedor)
            .Where(vendedor => !vendedoresExternosIds.Contains(vendedor.Id))
            .ToList();
        var vendedoresExternos = usuarios
            .Where(vendedor => vendedoresExternosIds.Contains(vendedor.Id))
            .ToList();

        return Ok(new { vendedores, vendedoresExternos });
    }

    [HttpGet("comisiones-vendedores")]
    public async Task<ActionResult<ReporteComisionesDto>> GetComisionesVendedores(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        [FromQuery] string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildFilterQuery(dateFrom, dateTo, vendedorId, NormalizeCommissionsScope(scope));
        var result = await _backendApiClient.GetResultAsync<ReporteComisionesDto>(
            $"api/Reportes/comisiones-vendedores?{filters}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo obtener el reporte." });
    }

    [HttpGet("comisiones-vendedores/{format}")]
    public async Task<IActionResult> DownloadComisionesVendedores(
        string format,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        [FromQuery] string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedFormat = format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? "pdf"
            : format.Equals("txt", StringComparison.OrdinalIgnoreCase)
                ? "txt"
                : "excel";
        var filters = BuildFilterQuery(dateFrom, dateTo, vendedorId, NormalizeCommissionsScope(scope));
        var result = await _backendApiClient.GetResultAsync<ExcelFileDto>(
            $"api/Reportes/comisiones-vendedores/{normalizedFormat}?{filters}",
            cancellationToken);

        if (!result.IsSuccess || result.Value is null || string.IsNullOrWhiteSpace(result.Value.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo preparar el archivo." });
        }

        var file = result.Value;
        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    [HttpGet("lotes-pago")]
    public async Task<ActionResult<IEnumerable<LotePagoDto>>> GetLotesPago(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<LotePagoDto>>(
            "api/Reportes/lotes-pago?tipoPago=COMISIONES",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar los lotes de pago." });
    }

    [HttpGet("envios")]
    public async Task<ActionResult<ReporteEnviosDto>> GetReporteEnvios(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? cliente = null,
        [FromQuery] string? metodoEntrega = null,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildEnviosFilterQuery(dateFrom, dateTo, cliente, metodoEntrega);
        var result = await _backendApiClient.GetResultAsync<ReporteEnviosDto>(
            $"api/Reportes/envios?{filters}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo cargar el reporte de envios." });
    }

    [HttpGet("lotes-pago/{id:int}/txt")]
    public async Task<IActionResult> DownloadLotePagoTxt(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<ExcelFileDto>(
            $"api/Reportes/lotes-pago/{id}/txt",
            cancellationToken);

        if (!result.IsSuccess || result.Value is null || string.IsNullOrWhiteSpace(result.Value.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo descargar el lote." });
        }

        var file = result.Value;
        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    [HttpPut("lotes-pago/{id:int}/estado")]
    public async Task<IActionResult> UpdateLotePagoEstado(int id, [FromBody] object request, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PutResultAsync<object>(
            $"api/Reportes/lotes-pago/{id}/estado",
            request,
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo actualizar el lote." });
    }

    private static string BuildFilterQuery(DateTime? dateFrom, DateTime? dateTo, int? vendedorId, string? scope = null)
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

        if (!string.IsNullOrWhiteSpace(scope))
        {
            filters.Add($"scope={Uri.EscapeDataString(scope)}");
        }

        return string.Join("&", filters);
    }

    private static bool IsPerfilVendedor(UsuarioDto usuario)
    {
        var perfil = $"{usuario.Perfil} {usuario.Perfiles}";
        return perfil.Contains("vendedor", StringComparison.OrdinalIgnoreCase) ||
            perfil.Contains("ventas", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExternalScope(string? scope)
    {
        return scope is not null &&
            (scope.Equals("externos", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("external", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("vendedores-externos", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeCommissionsScope(string? scope)
    {
        return IsExternalScope(scope) ? "externos" : "vendedores";
    }

    private static string BuildEnviosFilterQuery(DateTime? dateFrom, DateTime? dateTo, string? cliente, string? metodoEntrega)
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

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            filters.Add($"cliente={Uri.EscapeDataString(cliente)}");
        }

        if (!string.IsNullOrWhiteSpace(metodoEntrega))
        {
            filters.Add($"metodoEntrega={Uri.EscapeDataString(metodoEntrega)}");
        }

        return string.Join("&", filters);
    }
}
