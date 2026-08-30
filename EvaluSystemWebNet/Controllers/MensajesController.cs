using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MensajesController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public MensajesController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult<IEnumerable<MensajePendienteDto>>> GetPendientes(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<MensajePendienteDto>>("api/Mensajes/pendientes", cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar los mensajes pendientes." });
    }

    [HttpPost("{clave}/aceptar")]
    public async Task<IActionResult> Aceptar(string clave, CancellationToken cancellationToken)
    {
        var encoded = Uri.EscapeDataString(Uri.UnescapeDataString(clave));
        var result = await _backendApiClient.PostResultAsync<object>($"api/Mensajes/{encoded}/aceptar", new { }, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo aceptar el mensaje." });
    }
}
