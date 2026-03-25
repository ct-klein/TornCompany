using System.Net.Http;
using System.Text.Json;
using TornCompany.Models;

namespace TornCompany.Services;

public sealed class TornApiService : IDisposable
{
    private const string BaseUrl = "https://api.torn.com/company";

    private readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders = { { "User-Agent", "TornCompany/1.0" } }
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<CompanyListResponse?> GetCompaniesByTypeAsync(
        int typeId, string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{typeId}?selections=companies&key={apiKey}";
        return await FetchAsync<CompanyListResponse>(url, cancellationToken);
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(
        int userId, string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"https://api.torn.com/user/{userId}?selections=profile&key={apiKey}";
        return await FetchAsync<UserProfileResponse>(url, cancellationToken);
    }

    private async Task<T?> FetchAsync<T>(
        string url, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
