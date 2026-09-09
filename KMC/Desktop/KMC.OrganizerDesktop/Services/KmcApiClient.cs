using System.Net.Http.Headers;
using System.Net.Http.Json;
using KMC.OrganizerDesktop.Models;
using KMC.OrganizerDesktop.Session;

namespace KMC.OrganizerDesktop.Services;

public class KmcApiClient
{
    private readonly HttpClient _httpClient;

    public KmcApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(
                "https://localhost:7172/")
        };
    }

    public async Task<AuthResponse?> RegisterAsync(
        RegisterRequest request)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/Auth/register",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(error);
        }

        return await response.Content
            .ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/Auth/login",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(error);
        }

        return await response.Content
            .ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<EventResponse?> CreateEventAsync(
    CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(UserSession.Token))
        {
            throw new Exception(
                "You must login before creating an event.");
        }

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/Events");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                UserSession.Token);

        httpRequest.Content =
            JsonContent.Create(request);

        using var response =
            await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(error);
        }

        return await response.Content
            .ReadFromJsonAsync<EventResponse>();
    }

    public async Task<List<EventResponse>> GetMyEventsAsync()
    {
        if (string.IsNullOrWhiteSpace(UserSession.Token))
        {
            throw new Exception(
                "You must login before viewing your events.");
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/Events/my-events");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                UserSession.Token);

        using var response =
            await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(error);
        }

        return await response.Content
            .ReadFromJsonAsync<List<EventResponse>>()
            ?? new List<EventResponse>();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        if (string.IsNullOrWhiteSpace(UserSession.Token))
        {
            throw new Exception(
                "You must login before deleting an event.");
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/Events/{eventId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                UserSession.Token);

        using var response =
            await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(error);
        }
    }

    public async Task<EventResponse?> UpdateEventAsync(
    int eventId,
    CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(UserSession.Token))
        {
            throw new Exception(
                "You must login before updating an event.");
        }

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/Events/{eventId}");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                UserSession.Token);

        httpRequest.Content =
            JsonContent.Create(request);

        using var response =
            await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(error);
        }

        return await response.Content
            .ReadFromJsonAsync<EventResponse>();
    }
}