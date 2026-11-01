
using System.Globalization;
using InsightFlow.Core.Analytics;
using InsightFlow.Core.Models;

namespace InsightFlow.Core.Services;

public sealed class DatasetAnalyticsService
{
    private static readonly HashSet<string> MeasureRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Revenue",
            "Profit",
            "Cost",
            "Quantity",
            "UnitPrice"
        };

    private static readonly HashSet<string> DimensionRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Customer",
            "Transaction",
            "Product",
            "Category",
            "Region",
            "Identifier"
        };

    public DatasetSummaryResult CalculateSummary(
        Guid datasetId,
        IReadOnlyList<string> columns,
        IReadOnlyList<IReadOnlyList<string>> rows,
        IReadOnlyList<DatasetColumn> columnMetadata)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(columnMetadata);

        if (rows.Any(row => row.Count != columns.Count))
        {
            throw new InvalidDataException(
                "Rows have inconsistent column counts.");
        }

        var numericSummaries = new List<NumericColumnSummary>();
        var categoryBreakdowns = new List<CategoryBreakdown>();
        var timeSeries = new List<TimeSeriesPoint>();

        var metadataByIndex = columnMetadata
            .Where(column =>
                column.OrdinalPosition >= 0 &&
                column.OrdinalPosition < columns.Count)
            .ToDictionary(
                column => column.OrdinalPosition,
                column => column);

        string? GetRole(int index)
        {
            return metadataByIndex.TryGetValue(index, out var metadata)
                ? metadata.SemanticRole
                : null;
        }

        bool IsDateColumn(int index)
        {
            if (string.Equals(
                GetRole(index),
                "Date",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var values = rows
                .Select(row => row[index])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            return values.Count > 0 &&
                   values.All(value => TryParseDate(value).HasValue);
        }

        bool IsDimensionColumn(int index)
        {
            return DimensionRoles.Contains(GetRole(index) ?? string.Empty);
        }

        bool IsNumericMeasure(int index)
        {
            if (IsDateColumn(index) || IsDimensionColumn(index))
                return false;

            var values = rows
                .Select(row => row[index])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            if (values.Count == 0)
                return false;

            return values.All(value =>
                decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out _));
        }

        var dateColumns = Enumerable.Range(0, columns.Count)
            .Where(IsDateColumn)
            .ToHashSet();

        var measureColumns = Enumerable.Range(0, columns.Count)
            .Where(IsNumericMeasure)
            .ToList();

        foreach (var index in measureColumns)
        {
            var values = rows
                .Select(row => row[index])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => decimal.Parse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture))
                .ToList();

            if (values.Count == 0)
                continue;

            numericSummaries.Add(new NumericColumnSummary
            {
                ColumnName = columns[index],
                SemanticRole = GetRole(index),
                Sum = values.Sum(),
                Average = values.Average(),
                Minimum = values.Min(),
                Maximum = values.Max(),
                ValueCount = values.Count
            });
        }

        // Calculate business KPIs using semantic roles.
        NumericColumnSummary? GetMeasureSummary(string role)
        {
            return numericSummaries.FirstOrDefault(summary =>
                string.Equals(
                    summary.SemanticRole,
                    role,
                    StringComparison.OrdinalIgnoreCase));
        }

        int FindDimensionIndex(string role)
        {
            return Enumerable.Range(0, columns.Count)
                .FirstOrDefault(
                    index => string.Equals(
                        GetRole(index),
                        role,
                        StringComparison.OrdinalIgnoreCase),
                    -1);
        }

        long? GetDistinctCount(int columnIndex)
        {
            if (columnIndex < 0)
                return null;

            var distinctCount = rows
                .Select(row => row[columnIndex]?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .LongCount();

            return distinctCount > 0 ? distinctCount : null;
        }

        var revenue = GetMeasureSummary("Revenue");
        var profit = GetMeasureSummary("Profit");
        var quantity = GetMeasureSummary("Quantity");

        var transactionCount = GetDistinctCount(
            FindDimensionIndex("Transaction"));

        var customerCount = GetDistinctCount(
            FindDimensionIndex("Customer"));

        decimal? profitMargin = null;

        if (revenue is not null &&
            profit is not null &&
            revenue.Sum != 0)
        {
            profitMargin = profit.Sum / revenue.Sum * 100;
        }

        decimal? averageOrderValue = null;

        if (revenue is not null &&
            transactionCount.HasValue &&
            transactionCount.Value > 0)
        {
            averageOrderValue =
                revenue.Sum / transactionCount.Value;
        }

        var kpis = new BusinessKpiSummary
        {
            TotalRevenue = revenue?.Sum,
            TotalProfit = profit?.Sum,
            AverageRevenue = revenue?.Average,
            TotalQuantitySold = quantity?.Sum,
            ProfitMargin = profitMargin,
            TotalTransactions = transactionCount,
            AverageOrderValue = averageOrderValue,
            TotalCustomers = customerCount
        };

        var dateIndex = dateColumns
            .OrderBy(index => index)
            .FirstOrDefault(-1);

        var preferredMeasureRoles = new[]
        {
            "Revenue",
            "Profit",
            "Quantity"
        };

        var measureIndex = -1;

        foreach (var preferredRole in preferredMeasureRoles)
        {
            measureIndex = measureColumns.FirstOrDefault(
                index => string.Equals(
                    GetRole(index),
                    preferredRole,
                    StringComparison.OrdinalIgnoreCase),
                -1);

            if (measureIndex >= 0)
                break;
        }

        if (dateIndex >= 0 && measureIndex >= 0)
        {
            var dailyValues = new SortedDictionary<DateTime, decimal>();

            foreach (var row in rows)
            {
                var date = TryParseDate(row[dateIndex]);

                if (!date.HasValue ||
                    !decimal.TryParse(
                        row[measureIndex],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var value))
                {
                    continue;
                }

                var day = date.Value.Date;

                if (!dailyValues.ContainsKey(day))
                    dailyValues[day] = 0;

                dailyValues[day] += value;
            }

            timeSeries.AddRange(
                dailyValues.Select(item => new TimeSeriesPoint
                {
                    Period = item.Key,
                    Value = item.Value
                }));
        }

        for (var index = 0; index < columns.Count; index++)
        {
            if (dateColumns.Contains(index) ||
                measureColumns.Contains(index))
            {
                continue;
            }

            var values = rows
                .Select(row => row[index])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            if (values.Count == 0)
                continue;

            var groupedValues = values
                .GroupBy(value => value, StringComparer.Ordinal)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CategoryValue
                {
                    Label = group.Key,
                    Count = group.LongCount()
                })
                .ToList();

            var measures = new List<CategoryMeasure>();

            foreach (var measureColumnIndex in measureColumns)
            {
                var categoryGroups = rows
                    .Where(row =>
                        !string.IsNullOrWhiteSpace(row[index]) &&
                        !string.IsNullOrWhiteSpace(row[measureColumnIndex]))
                    .Select(row =>
                    {
                        var success = decimal.TryParse(
                            row[measureColumnIndex],
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var number);

                        return new
                        {
                            Category = row[index],
                            Success = success,
                            Number = number
                        };
                    })
                    .Where(item => item.Success)
                    .GroupBy(
                        item => item.Category,
                        StringComparer.Ordinal);

                foreach (var group in categoryGroups)
                {
                    var numbers = group
                        .Select(item => item.Number)
                        .ToList();

                    measures.Add(new CategoryMeasure
                    {
                        MeasureColumnName = columns[measureColumnIndex],
                        SemanticRole = GetRole(measureColumnIndex),
                        CategoryLabel = group.Key,
                        Total = numbers.Sum(),
                        Average = numbers.Average(),
                        ValueCount = numbers.Count
                    });
                }
            }

            categoryBreakdowns.Add(new CategoryBreakdown
            {
                ColumnName = columns[index],
                SemanticRole = GetRole(index),
                Values = groupedValues,
                Measures = measures
            });
        }

        return new DatasetSummaryResult
        {
            DatasetId = datasetId,
            TotalRows = rows.Count,
            TotalColumns = columns.Count,
            Kpis = kpis,
            NumericColumns = numericSummaries,
            CategoryBreakdowns = categoryBreakdowns,
            TimeSeries = timeSeries
        };
    }

    private static DateTime? TryParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string[] formats =
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "yyyy/MM/dd"
        };

        if (DateTime.TryParseExact(
            value.Trim(),
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var exactDate))
        {
            return exactDate;
        }

        if (DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }
}
