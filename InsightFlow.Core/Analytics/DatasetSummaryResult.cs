
namespace InsightFlow.Core.Analytics;

public sealed class DatasetSummaryResult
{
    public Guid DatasetId { get; init; }

    public int TotalRows { get; init; }

    public int TotalColumns { get; init; }

    public IReadOnlyList<NumericColumnSummary> NumericColumns { get; init; }
        = Array.Empty<NumericColumnSummary>();

    public IReadOnlyList<CategoryBreakdown> CategoryBreakdowns { get; init; }
        = Array.Empty<CategoryBreakdown>();

    public IReadOnlyList<TimeSeriesPoint> TimeSeries { get; init; }
        = Array.Empty<TimeSeriesPoint>();
}

public sealed class NumericColumnSummary
{
    public string ColumnName { get; init; } = string.Empty;

    public decimal Sum { get; init; }

    public decimal Average { get; init; }

    public decimal Minimum { get; init; }

    public decimal Maximum { get; init; }

    public long ValueCount { get; init; }
}


public sealed class CategoryBreakdown
{
    public string ColumnName { get; init; } = string.Empty;

    public IReadOnlyList<CategoryValue> Values { get; init; }
        = Array.Empty<CategoryValue>();

    public IReadOnlyList<CategoryMeasure> Measures { get; init; }
        = Array.Empty<CategoryMeasure>();
}

public sealed class CategoryMeasure
{
    public string MeasureColumnName { get; init; } = string.Empty;

    public string CategoryLabel { get; init; } = string.Empty;

    public decimal Total { get; init; }

    public decimal Average { get; init; }

    public long ValueCount { get; init; }
}


public sealed class CategoryValue
{
    public string Label { get; init; } = string.Empty;

    public long Count { get; init; }

    public decimal? Total { get; init; }
}

public sealed class TimeSeriesPoint
{
    public DateTime Period { get; init; }

    public decimal Value { get; init; }
}
