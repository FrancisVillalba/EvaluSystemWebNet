using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ControlController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public ControlController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ControlPedidoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<ControlPedidoDto>>("api/Control", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage ?? "No se pudieron cargar pedidos en control." });
    }

    [HttpPost("{id:int}/aprobar")]
    public async Task<ActionResult<ControlPedidoDto>> Aprobar(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<ControlPedidoDto>($"api/Control/{id}/aprobar", new { }, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage ?? "No se pudo aprobar el pedido." });
    }
}