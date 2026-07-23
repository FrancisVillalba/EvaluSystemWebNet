using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public DashboardController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var dashboard = await _backendApiClient.GetAsync<DashboardSummaryDto>(
            "api/VentasImpresion/dashboard",
            cancellationToken);

        if (dashboard is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener el dashboard desde EvaluSystemBack." });
        }

        return Ok(dashboard);
    }

    [HttpGet("pedidos")]
    public async Task<IActionResult> GetPedidos(
        [FromQuery] string tipo,
        [FromQuery] string? cliente,
        CancellationToken cancellationToken)
    {
        var query = $"tipo={Uri.EscapeDataString(tipo ?? string.Empty)}";
        if (!string.IsNullOrWhiteSpace(cliente))
        {
            query += $"&cliente={Uri.EscapeDataString(cliente)}";
        }

        var result = await _backendApiClient.GetResultAsync<IEnumerable<DashboardPedidoDto>>(
            $"api/VentasImpresion/dashboard/pedidos?{query}", cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage ?? "No se pudo cargar el detalle." });
    }
}
