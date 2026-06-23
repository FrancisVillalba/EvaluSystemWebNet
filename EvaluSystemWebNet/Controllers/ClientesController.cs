using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public ClientesController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteView>>> GetAll(CancellationToken cancellationToken)
    {
        var clientes = await _backendApiClient.GetAsync<IEnumerable<ClienteDto>>("api/Clientes", cancellationToken);

        if (clientes is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener clientes desde EvaluSystemBack." });
        }

        return Ok(clientes.Select(ToView));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _backendApiClient.DeleteAsync($"api/Clientes/{id}", cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status502BadGateway);
    }

    private static ClienteView ToView(ClienteDto cliente)
    {
        return new ClienteView(
            cliente.Id,
            cliente.Nombre ?? string.Empty,
            cliente.Documento ?? string.Empty,
            cliente.NroTelefono ?? string.Empty,
            cliente.Email ?? string.Empty,
            cliente.DatosEnvio?.Ciudad ?? string.Empty,
            cliente.DatosEnvio is not null,
            cliente.Estado == false ? "Inactivo" : "Activo");
    }
}
