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
        HttpContext.Session.SetString("BackendExpiresAt", response.ExpiresAt.ToString("O"));
        HttpContext.Session.SetString("BackendRefreshToken", response.RefreshToken);
        HttpContext.Session.SetString("BackendRefreshExpiresAt", response.RefreshExpiresAt.ToString("O"));
        HttpContext.Session.SetString("BackendPermissions", JsonSerializer.Serialize(response.Permisos));

        return Ok(new
        {
            response.UsuarioId,
            response.Usuario,
            response.Persona,
            response.ExpiresAt,
            RedirectUrl = $"{Request.PathBase}{ResolveRedirectUrl(response.Permisos)}"
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("BackendUserId");
        var userName = HttpContext.Session.GetString("BackendUserName");

        if (!userId.HasValue || string.IsNullOrWhiteSpace(userName))
        {
            return Unauthorized(new { message = "La sesion caduco. Inicie sesion nuevamente." });
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "Complete la contrasena actual y la nueva contrasena." });
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new { message = "La confirmacion no coincide con la nueva contrasena." });
        }

        var loginResult = await _backendApiClient.PostResultAsync<LoginResponse>(
            "api/Auth/login",
            new LoginRequest(userName, request.CurrentPassword),
            cancellationToken);

        if (!loginResult.IsSuccess || loginResult.Value is null)
        {
            return BadRequest(new { message = "La contrasena actual no es correcta." });
        }

        var userResult = await _backendApiClient.GetResultAsync<UsuarioDto>($"api/Usuarios/{userId.Value}", cancellationToken);
        if (!userResult.IsSuccess || userResult.Value is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = userResult.ErrorMessage ?? "No se pudo obtener el usuario actual." });
        }

        var user = userResult.Value;
        var updatePayload = new
        {
            NombreUsuario = user.NombreUsuario ?? userName,
            Pass = request.NewPassword,
            user.PersonaId,
            PerfilIds = user.PerfilIds,
            Estado = user.Estado ?? true
        };

        var updateResult = await _backendApiClient.PutResultAsync<JsonElement>($"api/Usuarios/{userId.Value}", updatePayload, cancellationToken);
        return updateResult.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = updateResult.ErrorMessage ?? "No se pudo cambiar la contrasena." });
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
        if (permission.FormularioPadre?.Equals("Reportes", StringComparison.OrdinalIgnoreCase) == true ||
            permission.Formulario.StartsWith("Reporte ", StringComparison.OrdinalIgnoreCase))
        {
            return "/reportes";
        }

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

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
