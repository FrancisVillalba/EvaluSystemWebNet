using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly IBackendApiClient _backend;
    public NotificacionesController(IBackendApiClient backend) => _backend = backend;

    [HttpGet]
    public async Task<ActionResult<NotificacionesResumenDto>> Get(CancellationToken cancellationToken)
    {
        var result = await _backend.GetResultAsync<NotificacionesResumenDto>("api/Notificaciones", cancellationToken);
        return result.IsSuccess && result.Value is not null ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    [HttpPut("{id:long}/leer")]
    public async Task<IActionResult> Leer(long id, CancellationToken cancellationToken)
    {
        var result = await _backend.PutResultAsync<object>($"api/Notificaciones/{id}/leer", new { }, cancellationToken);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    [HttpPut("leer-todas")]
    public async Task<IActionResult> LeerTodas(CancellationToken cancellationToken)
    {
        var result = await _backend.PutResultAsync<object>("api/Notificaciones/leer-todas", new { }, cancellationToken);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }
}