using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EvaluSystemWebNet.Services;

public interface IBackendApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
}

public class BackendApiClient : IBackendApiClient
{
    private const string TokenSessionKey = "BackendAccessToken";
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public BackendApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<T?> PostAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        var token = _httpContextAccessor.HttpContext?.Session.GetString(TokenSessionKey);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    private async Task<T?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
    }
}
