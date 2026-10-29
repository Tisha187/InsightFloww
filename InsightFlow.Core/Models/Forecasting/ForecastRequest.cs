
namespace InsightFlow.Core.Models.Forecasting;

public sealed class ForecastRequest
{
    public List<string> Dates { get; set; } = [];

    public List<double> Values { get; set; } = [];

    public int Horizon { get; set; }

    public string Frequency { get; set; } = string.Empty;
}
