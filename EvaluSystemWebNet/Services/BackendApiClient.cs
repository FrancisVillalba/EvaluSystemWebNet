using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EvaluSystemWebNet.Services;

public interface IBackendApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default);
    Task<BackendApiResult<T>> GetResultAsync<T>(string path, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<BackendApiResult<T>> PostResultAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<T?> PutAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<BackendApiResult<T>> PutResultAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<BackendApiResult<T>> PostFileResultAsync<T>(string path, IFormFile file, IReadOnlyDictionary<string, string?>? fields = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
}

public record BackendApiResult<T>(bool IsSuccess, T? Value, string? ErrorMessage, int StatusCode);

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

    public async Task<BackendApiResult<T>> GetResultAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResultAsync<T>(response, cancellationToken);
    }

    public async Task<T?> PostAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = JsonContent(body);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<BackendApiResult<T>> PostResultAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = JsonContent(body);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResultAsync<T>(response, cancellationToken);
    }

    public async Task<T?> PutAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, path);
        request.Content = JsonContent(body);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<BackendApiResult<T>> PutResultAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, path);
        request.Content = JsonContent(body);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResultAsync<T>(response, cancellationToken);
    }

    public async Task<BackendApiResult<T>> PostFileResultAsync<T>(string path, IFormFile file, IReadOnlyDictionary<string, string?>? fields = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path);
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();

        var fileContent = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        }

        content.Add(fileContent, "archivo", file.FileName);
        if (fields is not null)
        {
            foreach (var field in fields)
            {
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    content.Add(new StringContent(field.Value), field.Key);
                }
            }
        }

        request.Content = content;
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResultAsync<T>(response, cancellationToken);
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

    private StringContent JsonContent(object body)
    {
        return new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");
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

    private async Task<BackendApiResult<T>> ReadResultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new BackendApiResult<T>(true, default, null, statusCode);
            }

            try
            {
                var value = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
                return new BackendApiResult<T>(true, value, null, statusCode);
            }
            catch (Exception ex)
            {
                return new BackendApiResult<T>(false, default, $"No se pudo leer la respuesta de EvaluSystemBack: {ex.Message}", statusCode);
            }
        }

        string? message = null;
        try
        {
            var error = await JsonSerializer.DeserializeAsync<BackendErrorResponse>(stream, _jsonOptions, cancellationToken);
            message = error?.Message;
        }
        catch
        {
            // Keep a generic message if the back did not return JSON.
        }

        return new BackendApiResult<T>(false, default, message ?? "No se pudo completar la operacion en EvaluSystemBack.", statusCode);
    }

    private record BackendErrorResponse(string? Message);
}
