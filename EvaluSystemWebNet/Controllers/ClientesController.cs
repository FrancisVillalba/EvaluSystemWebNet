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
        [FromQuery] string? search = null,
        [FromQuery] int? ciudadId = null,
        [FromQuery] bool? estado = null,
        [FromQuery] int? transportadoraId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (ciudadId.HasValue)
        {
            filters.Add($"ciudadId={ciudadId.Value}");
        }

        if (estado.HasValue)
        {
            filters.Add($"estado={estado.Value.ToString().ToLowerInvariant()}");
        }

        if (transportadoraId.HasValue)
        {
            filters.Add($"transportadoraId={transportadoraId.Value}");
        }

        var result = await _backendApiClient.GetResultAsync<PagedResponse<ClienteDto>>(
            $"api/Clientes?{string.Join("&", filters)}",
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = result.ErrorMessage ?? "No se pudo obtener clientes desde EvaluSystemBack."
            });
        }

        var clientes = result.Value;
        return Ok(new PagedView<ClienteView>(
            clientes.Items.Select(ToView),
            clientes.Page,
            clientes.PageSize,
            clientes.TotalItems,
            clientes.TotalPages));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var cliente = await _backendApiClient.GetAsync<ClienteDto>($"api/Clientes/{id}", cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<ClienteOptionsDto>> GetOptions(CancellationToken cancellationToken)
    {
        var tiposDocumentoTask = _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/TiposDocumento", cancellationToken);
        var tiposClienteTask = _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/TiposCliente", cancellationToken);
        var transportadorasTask = _backendApiClient.GetAsync<IEnumerable<TransportadoraDto>>("api/Transportadoras", cancellationToken);
        var departamentosTask = _backendApiClient.GetAsync<IEnumerable<DepartamentoDto>>("api/Departamentos", cancellationToken);
        var ciudadesTask = _backendApiClient.GetAsync<IEnumerable<CiudadDto>>("api/Ciudades", cancellationToken);

        await Task.WhenAll(tiposDocumentoTask, tiposClienteTask, transportadorasTask, departamentosTask, ciudadesTask);

        return Ok(new ClienteOptionsDto(
            await tiposDocumentoTask ?? [],
            await tiposClienteTask ?? [],
            await transportadorasTask ?? [],
            await departamentosTask ?? [],
            await ciudadesTask ?? []));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(
        [FromBody] ClienteSaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<ClienteDto>(
            "api/Clientes",
            request.Cliente,
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo crear el cliente." });
        }

        var cliente = result.Value;

        if (request.DatosEnvio is not null)
        {
            var envio = request.DatosEnvio with { ClienteId = cliente.Id };
            await _backendApiClient.PostAsync<ClienteDatosEnvioDto>("api/ClienteDatosEnvio", envio, cancellationToken);
        }

        var completo = await _backendApiClient.GetAsync<ClienteDto>($"api/Clientes/{cliente.Id}", cancellationToken);
        return Ok(completo ?? cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ClienteSaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PutResultAsync<ClienteDto>(
            $"api/Clientes/{id}",
            request.Cliente,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
        }

        if (request.DatosEnvio is not null)
        {
            var existing = await _backendApiClient.GetAsync<ClienteDatosEnvioDto>($"api/ClienteDatosEnvio/cliente/{id}", cancellationToken);
            var envio = request.DatosEnvio with { ClienteId = id };

            if (existing is null)
            {
                await _backendApiClient.PostAsync<ClienteDatosEnvioDto>("api/ClienteDatosEnvio", envio, cancellationToken);
            }
            else
            {
                await _backendApiClient.PutResultAsync<ClienteDatosEnvioDto>($"api/ClienteDatosEnvio/{existing.Id}", envio, cancellationToken);
            }
        }

        return NoContent();
    }

    [HttpDelete("{id:int}/datos-envio")]
    public async Task<IActionResult> DeleteShippingData(int id, CancellationToken cancellationToken)
    {
        var existing = await _backendApiClient.GetAsync<ClienteDatosEnvioDto>($"api/ClienteDatosEnvio/cliente/{id}", cancellationToken);
        if (existing is null)
        {
            return NoContent();
        }

        var deleted = await _backendApiClient.DeleteAsync($"api/ClienteDatosEnvio/{existing.Id}", cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo eliminar el envio." });
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
            cliente.DatosEnvio?.CiudadId,
            cliente.DatosEnvio?.Ciudad ?? string.Empty,
            cliente.DatosEnvio?.TransportadoraId,
            cliente.DatosEnvio?.Transportadora ?? string.Empty,
            cliente.DatosEnvio is not null,
            cliente.Estado == false ? "Inactivo" : "Activo");
    }

}

public record ClienteSaveRequest(ClienteRequest Cliente, ClienteDatosEnvioRequest? DatosEnvio);
