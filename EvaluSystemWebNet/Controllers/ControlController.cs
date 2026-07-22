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


    [HttpGet("{detalleId:int}/archivo")]
    public async Task<IActionResult> DescargarArchivo(int detalleId, CancellationToken cancellationToken)
    {
        var file = await _backendApiClient.GetAsync<ExcelFileDto>($"api/Control/{detalleId}/archivo", cancellationToken);
        if (file is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo descargar el archivo." });
        }

        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }
    [HttpPost("{id:int}/aprobar")]
    public async Task<ActionResult<ControlPedidoDto>> Aprobar(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<ControlPedidoDto>($"api/Control/{id}/aprobar", new { }, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage ?? "No se pudo aprobar el pedido." });
    }

    [HttpPost("{id:int}/rechazar")]
    public async Task<ActionResult<ControlPedidoDto>> Rechazar(int id, [FromBody] EliminarPedidoRequest request, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<ControlPedidoDto>($"api/Control/{id}/rechazar", request, cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage ?? "No se pudo rechazar el pedido." });
    }}