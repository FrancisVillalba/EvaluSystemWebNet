using System.Text.Json;
using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdministracionController : ControllerBase
{
    private static readonly Dictionary<string, AdminModule> Modules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["usuarios"] = new("api/Usuarios", true, true, true),
        ["personas"] = new("api/Personas", true, true, true),
        ["perfiles"] = new("api/Perfiles", true, true, true),
        ["transportadoras"] = new("api/Transportadoras", true, true, true),
        ["productos"] = new("api/Productos", true, true, true),
        ["maquinas"] = new("api/TiposMaquina", true, true, true),
        ["formasPago"] = new("api/FormasPago", true, true, true),
        ["estadosPago"] = new("api/EstadosPago", true, true, true),
        ["estadosVenta"] = new("api/EstadosVenta", true, true, true),
        ["tiposDocumento"] = new("api/TiposDocumento", true, true, true),
        ["tiposCliente"] = new("api/TiposCliente", true, true, true),
        ["configuraciones"] = new("api/Configuraciones", true, true, true),
        ["departamentos"] = new("api/Departamentos", true, true, true),
        ["ciudades"] = new("api/Ciudades", true, true, true)
    };

    private readonly IBackendApiClient _backendApiClient;

    public AdministracionController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<AdminOptionsDto>> GetOptions(CancellationToken cancellationToken)
    {
        var perfilesTask = _backendApiClient.GetAsync<IEnumerable<PerfilDto>>("api/Perfiles", cancellationToken);
        var personasTask = _backendApiClient.GetAsync<IEnumerable<PersonaDto>>("api/Personas", cancellationToken);
        var maquinasTask = _backendApiClient.GetAsync<IEnumerable<TipoMaquinaDto>>("api/TiposMaquina", cancellationToken);
        var departamentosTask = _backendApiClient.GetAsync<IEnumerable<DepartamentoDto>>("api/Departamentos", cancellationToken);
        var tiposDocumentoTask = _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/TiposDocumento", cancellationToken);

        await Task.WhenAll(perfilesTask, personasTask, maquinasTask, departamentosTask, tiposDocumentoTask);

        return Ok(new AdminOptionsDto(
            await perfilesTask ?? [],
            await personasTask ?? [],
            await maquinasTask ?? [],
            await departamentosTask ?? [],
            await tiposDocumentoTask ?? []));
    }

    [HttpGet("{module}")]
    public async Task<IActionResult> GetAll(string module, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        var result = await _backendApiClient.GetResultAsync<JsonElement>(config.Endpoint, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{module}")]
    public async Task<IActionResult> Create(string module, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!config.CanCreate)
        {
            return BadRequest(new { message = "Este modulo es solo lectura." });
        }

        var result = await _backendApiClient.PostResultAsync<JsonElement>(config.Endpoint, body, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("{module}/{id}")]
    public async Task<IActionResult> Update(string module, string id, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!config.CanEdit)
        {
            return BadRequest(new { message = "Este modulo es solo lectura." });
        }

        var result = await _backendApiClient.PutResultAsync<JsonElement>($"{config.Endpoint}/{Uri.EscapeDataString(id)}", body, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    [HttpDelete("{module}/{id}")]
    public async Task<IActionResult> Delete(string module, string id, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!config.CanDelete)
        {
            return BadRequest(new { message = "Este modulo es solo lectura." });
        }

        var deleted = await _backendApiClient.DeleteAsync($"{config.Endpoint}/{Uri.EscapeDataString(id)}", cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo eliminar el registro." });
    }

    [HttpGet("permisos/formularios")]
    public async Task<IActionResult> GetFormularios(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<JsonElement>("api/Permisos/formularios", cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("permisos/perfil/{perfilId:int}")]
    public async Task<IActionResult> GetPermisosPerfil(int perfilId, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<JsonElement>($"api/Permisos/perfil/{perfilId}", cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("permisos/perfil-formulario")]
    public async Task<IActionResult> SavePermiso([FromBody] PerfilFormularioPermisoRequest request, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<PerfilFormularioPermisoDto>("api/Permisos/perfil-formulario", request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    private static bool TryGetModule(string module, out AdminModule config)
    {
        return Modules.TryGetValue(module, out config!);
    }

    private IActionResult ToActionResult<T>(BackendApiResult<T> result)
    {
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    private record AdminModule(string Endpoint, bool CanCreate, bool CanEdit, bool CanDelete);
}
