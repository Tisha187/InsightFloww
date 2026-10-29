
using System.Net.Http.Json;
using System.Text.Json;
using InsightFlow.Core.Models.Forecasting;

namespace InsightFlow.Core.Services.Forecasting;

public sealed class ForecastingClient : IForecastingClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public ForecastingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ForecastResponse> GenerateForecastAsync(
        ForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "forecast",
            request,
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if ((int)response.StatusCode == 422)
            {
                throw new InvalidOperationException(
                    $"Forecast input was rejected: {errorBody}");
            }

            throw new HttpRequestException(
                $"Forecasting service returned HTTP " +
                $"{(int)response.StatusCode}: {errorBody}",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<ForecastResponse>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException(
            "The forecasting service returned an empty response.");
    }
}
