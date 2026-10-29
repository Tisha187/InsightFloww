
using InsightFlow.Core.Models.Forecasting;

namespace InsightFlow.Core.Services.Forecasting;

public interface IForecastingClient
{
    Task<ForecastResponse> GenerateForecastAsync(
        ForecastRequest request,
        CancellationToken cancellationToken = default);
}
