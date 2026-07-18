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
    var token = context.Session.GetString("BackendAccessToken");
    var expiresAtText = context.Session.GetString("BackendExpiresAt");
    var sessionExpired = DateTime.TryParse(
        expiresAtText,
        null,
        System.Globalization.DateTimeStyles.RoundtripKind,
        out var expiresAt) && expiresAt <= DateTime.UtcNow;

    if (sessionExpired)
    {
        context.Session.Clear();
    }

    if (isApiRequest && !isAuthRequest && (string.IsNullOrWhiteSpace(token) || sessionExpired))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = "La sesion caduco. Inicie sesion nuevamente." });
        return;
    }

    await next();
});
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
