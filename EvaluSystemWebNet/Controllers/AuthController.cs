using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;

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

        return Ok(new
        {
            response.UsuarioId,
            response.Usuario,
            response.Persona,
            response.ExpiresAt
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }
}
