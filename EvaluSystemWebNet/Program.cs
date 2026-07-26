using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
const long maxUploadBytes = 500L * 1024L * 1024L;

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes;
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = maxUploadBytes;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddHttpClient<IBackendApiClient, BackendApiClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["BackendApi:BaseUrl"] ?? "https://localhost:44318";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.Use(async (context, next) =>
{
    var isApiRequest = context.Request.Path.StartsWithSegments("/api");
    var isAuthRequest = context.Request.Path.StartsWithSegments("/api/auth");

    if (isApiRequest && !isAuthRequest)
    {
        var token = context.Session.GetString("BackendAccessToken");
        var expiresAtText = context.Session.GetString("BackendExpiresAt");
        var accessTokenExpired = !DateTime.TryParse(
            expiresAtText,
            null,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out var expiresAt) || expiresAt <= DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(token) || accessTokenExpired)
        {
            var refreshToken = context.Session.GetString("BackendRefreshToken");
            var refreshExpiresAtText = context.Session.GetString("BackendRefreshExpiresAt");
            var refreshTokenValid = !string.IsNullOrWhiteSpace(refreshToken)
                && DateTime.TryParse(
                    refreshExpiresAtText,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var refreshExpiresAt)
                && refreshExpiresAt > DateTime.UtcNow;

            LoginResponse? refreshedSession = null;
            if (refreshTokenValid)
            {
                try
                {
                    var backendApiClient = context.RequestServices.GetRequiredService<IBackendApiClient>();
                    refreshedSession = await backendApiClient.PostAsync<LoginResponse>(
                        "api/Auth/refresh",
                        new RefreshTokenRequest(refreshToken!),
                        context.RequestAborted);
                }
                catch (HttpRequestException)
                {
                    // The session is closed below when the backend cannot renew it.
                }
            }

            if (refreshedSession is null)
            {
                context.Session.Clear();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "La sesion caduco. Inicie sesion nuevamente." });
                return;
            }

            context.Session.SetString("BackendAccessToken", refreshedSession.Token);
            context.Session.SetString("BackendExpiresAt", refreshedSession.ExpiresAt.ToString("O"));
            context.Session.SetString("BackendRefreshToken", refreshedSession.RefreshToken);
            context.Session.SetString("BackendRefreshExpiresAt", refreshedSession.RefreshExpiresAt.ToString("O"));
            context.Session.SetString("BackendUserName", refreshedSession.Usuario);
            context.Session.SetInt32("BackendUserId", refreshedSession.UsuarioId);
            context.Session.SetString("BackendPermissions", System.Text.Json.JsonSerializer.Serialize(refreshedSession.Permisos));
        }
    }

    await next();
});
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
