using System.Text;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;

namespace Timberborn.Infrastructure.Services;

public class LeverCaller : ILeverCaller
{
    private readonly HttpClient _httpClient;

    public LeverCaller(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<LeverCallResult> CallAsync(Lever lever, string state)
    {
        var url = state == "off" ? lever.UrlOff : lever.UrlOn;
        if (string.IsNullOrWhiteSpace(url))
            return new LeverCallResult(false, null, null, $"URL ({state}) is not configured for this lever.");

        try
        {
            var method = lever.HttpMethod.ToUpperInvariant() switch
            {
                "GET" => HttpMethod.Get,
                "PUT" => HttpMethod.Put,
                "PATCH" => HttpMethod.Patch,
                _ => HttpMethod.Post
            };

            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrWhiteSpace(lever.BodyTemplate) && method != HttpMethod.Get)
                request.Content = new StringContent(lever.BodyTemplate, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return new LeverCallResult(response.IsSuccessStatusCode, (int)response.StatusCode, body, null);
        }
        catch (Exception ex)
        {
            return new LeverCallResult(false, null, null, ex.Message);
        }
    }
}
