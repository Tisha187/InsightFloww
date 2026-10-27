
using System.Globalization;
using InsightFlow.Core.Analytics;

namespace InsightFlow.Core.Services;

public sealed class DatasetAnalyticsService
{
    public DatasetSummaryResult CalculateSummary(
        Guid datasetId,
        IReadOnlyList<string> columns,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);

        if (rows.Any(row => row.Count != columns.Count))
        {
            throw new InvalidDataException(
                "Rows have inconsistent column counts.");
        }

        var numericSummaries = new List<NumericColumnSummary>();
        var categoryBreakdowns = new List<CategoryBreakdown>();
        var timeSeries = new List<TimeSeriesPoint>();

        // Detect date columns, excluding columns containing only numbers.
        var dateColumns = new HashSet<int>();

        for (var index = 0; index < columns.Count; index++)
        {
            var values = rows
                .Select(row => row[index])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            if (values.Count == 0)
                continue;

            var numericCount = values.Count(value =>
                decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out _));

            if (numericCount == values.Count)
                continue;

            var parsedDateCount = values.Count(value =>
                TryParseDate(value).HasValue);

            if (parsedDateCount == values.Count)
                dateColumns.Add(index);
        }

        // Find the first date column.
        var dateIndex = dateColumns
            .OrderBy(index => index)
            .FirstOrDefault(-1);

        // Find a numeric measure named Revenue for the time series.
        var revenueIndex = -1;

        for (var index = 0; index < columns.Count; index++)
        {
            if (string.Equals(
                columns[index].Trim(),
                "Revenue",
                StringComparison.OrdinalIgnoreCase))
            {
                revenueIndex = index;
                break;
            }
        }

        // Aggregate revenue by date when both columns are available.
        if (dateIndex >= 0 && revenueIndex >= 0)
        {
            var dailyRevenue = new SortedDictionary<DateTime, decimal>();

            foreach (var row in rows)
            {
                var date = TryParseDate(row[dateIndex]);

                if (!date.HasValue ||
                    !decimal.TryParse(
                        row[revenueIndex],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var revenue))
                {
                    continue;
                }

                var day = date.Value.Date;

                if (!dailyRevenue.ContainsKey(day))
                    dailyRevenue[day] = 0;

                dailyRevenue[day] += revenue;
            }

            timeSeries.AddRange(
                dailyRevenue.Select(item => new TimeSeriesPoint
                {
                    Period = item.Key,
                    Value = item.Value
                }));
        }

        // Classify numeric and categorical columns.
        for (var index = 0; index < columns.Count; index++)
        {
            if (dateColumns.Contains(index))
                continue;

            var values = rows
                .Select(row => row[index])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            if (values.Count == 0)
                continue;

            var parsedNumbers = new List<decimal>();
            var allNumeric = true;

            foreach (var value in values)
            {
                if (decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var number))
                {
                    parsedNumbers.Add(number);
                }
                else
                {
                    allNumeric = false;
                    break;
                }
            }

            if (allNumeric)
            {
                numericSummaries.Add(new NumericColumnSummary
                {
                    ColumnName = columns[index],
                    Sum = parsedNumbers.Sum(),
                    Average = parsedNumbers.Average(),
                    Minimum = parsedNumbers.Min(),
                    Maximum = parsedNumbers.Max(),
                    ValueCount = parsedNumbers.Count
                });

                continue;
            }

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

            // Calculate each numeric measure separately for each category.
            for (var measureIndex = 0;
                 measureIndex < columns.Count;
                 measureIndex++)
            {
                if (measureIndex == index ||
                    dateColumns.Contains(measureIndex))
                {
                    continue;
                }

                var numericValues = rows
                    .Where(row =>
                        !string.IsNullOrWhiteSpace(row[index]) &&
                        !string.IsNullOrWhiteSpace(row[measureIndex]))
                    .Select(row =>
                    {
                        var success = decimal.TryParse(
                            row[measureIndex],
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
                    .ToList();

                // Skip columns containing non-numeric values.
                if (numericValues.Count == 0 ||
                    numericValues.Any(item => !item.Success))
                {
                    continue;
                }

                var categoryGroups = numericValues
                    .GroupBy(
                        item => item.Category,
                        StringComparer.Ordinal);

                foreach (var group in categoryGroups)
                {
                    var groupNumbers = group
                        .Select(item => item.Number)
                        .ToList();

                    measures.Add(new CategoryMeasure
                    {
                        MeasureColumnName = columns[measureIndex],
                        CategoryLabel = group.Key,
                        Total = groupNumbers.Sum(),
                        Average = groupNumbers.Average(),
                        ValueCount = groupNumbers.Count
                    });
                }
            }

            categoryBreakdowns.Add(new CategoryBreakdown
            {
                ColumnName = columns[index],
                Values = groupedValues,
                Measures = measures
            });
        }

        return new DatasetSummaryResult
        {
            DatasetId = datasetId,
            TotalRows = rows.Count,
            TotalColumns = columns.Count,
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
