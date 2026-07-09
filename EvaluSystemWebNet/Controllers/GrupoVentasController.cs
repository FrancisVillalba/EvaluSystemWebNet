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
    public async Task<IActionResult> GetMiEquipo([FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, CancellationToken cancellationToken = default)
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

        var result = await _backendApiClient.GetResultAsync<GrupoVentaEquipoDto>(
            $"api/GruposVenta/mi-equipo?{string.Join("&", filters)}",
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }
}
