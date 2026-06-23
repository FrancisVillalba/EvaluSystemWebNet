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
    public async Task<ActionResult<PagedView<ClienteView>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var clientes = await _backendApiClient.GetAsync<PagedResponse<ClienteDto>>(
            $"api/Clientes?page={page}&pageSize={pageSize}",
            cancellationToken);

        if (clientes is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener clientes desde EvaluSystemBack." });
        }

        return Ok(new PagedView<ClienteView>(
            clientes.Items.Select(ToView),
            clientes.Page,
            clientes.PageSize,
            clientes.TotalItems,
            clientes.TotalPages));
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
