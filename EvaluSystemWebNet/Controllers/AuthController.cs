using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EvaluSystemWebNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string TokenSessionKey = "BackendAccessToken";
    private readonly IBackendApiClient _backendApiClient;

    public AuthController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _backendApiClient.PostAsync<LoginResponse>("api/Auth/login", request, cancellationToken);

        if (response is null)
        {
            return Unauthorized(new { message = "Usuario o contrasena incorrectos." });
        }

        HttpContext.Session.SetString(TokenSessionKey, response.Token);
        HttpContext.Session.SetString("BackendUserName", response.Usuario);
        HttpContext.Session.SetInt32("BackendUserId", response.UsuarioId);
        HttpContext.Session.SetString("BackendPermissions", JsonSerializer.Serialize(response.Permisos));

        return Ok(new
        {
            response.UsuarioId,
            response.Usuario,
            response.Persona,
            response.ExpiresAt,
            RedirectUrl = ResolveRedirectUrl(response.Permisos)
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }

    private static string ResolveRedirectUrl(IEnumerable<PerfilFormularioPermisoDto> permissions)
    {
        var visible = permissions
            .Where(x => x.PuedeVer)
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Formulario)
            .ToList();

        return visible.Select(RouteForPermission).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "/dashboard";
    }

    private static string? RouteForPermission(PerfilFormularioPermisoDto permission)
    {
        return permission.Formulario switch
        {
            "Tablero" => "/dashboard",
            "Pedidos" => "/pedidos",
            "Grupo de ventas" => "/grupo-ventas",
            "Clientes" => "/clientes",
            "Impresiones" => "/impresiones",
            "Envio" => "/delivery",
            "Reportes" => "/reportes",
            "Administracion" => "/administracion",
            _ => string.IsNullOrWhiteSpace(permission.Ruta) ? null : permission.Ruta
        };
    }
}
