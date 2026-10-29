
namespace InsightFlow.Core.Models.Forecasting;

public sealed class ForecastResponse
{
    public string ModelUsed { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public int Horizon { get; set; }

    public List<HistoricalPoint> Historical { get; set; } = [];

    public List<ForecastPrediction> Forecasts { get; set; } = [];

    public ForecastEvaluation Evaluation { get; set; } = new();
}

public sealed class HistoricalPoint
{
    public DateOnly Date { get; set; }

    public double Actual { get; set; }
}

public sealed class ForecastPrediction
{
    public DateOnly Date { get; set; }

    public double Predicted { get; set; }
}

public sealed class ForecastEvaluation
{
    public int HoldoutObservations { get; set; }

    public Dictionary<string, ForecastMetrics> Models { get; set; } = [];
}

public sealed class ForecastMetrics
{
    public double Mae { get; set; }

    public double Rmse { get; set; }
}
