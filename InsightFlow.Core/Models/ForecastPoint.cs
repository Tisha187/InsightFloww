using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class ForecastPoint
{
    public long Id { get; set; }

    public Guid ForecastRunId { get; set; }

    public DateOnly ForecastDate { get; set; }

    public decimal PredictedValue { get; set; }

    public decimal? LowerBound { get; set; }

    public decimal? UpperBound { get; set; }

    public virtual ForecastRun ForecastRun { get; set; } = null!;
}
