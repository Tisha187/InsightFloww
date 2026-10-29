
using System.Globalization;
using InsightFlow.Core.Models.Forecasting;
using System.Text.Json;
using InsightFlow.Core.Repositories;

namespace InsightFlow.Core.Services.Forecasting;

public sealed class ForecastingService
{
    private readonly InsightFlow.Core.Services.IDatasetService _datasetService;
    private readonly IForecastingClient _forecastingClient;

    private readonly IForecastRepository _forecastRepository;

    private static readonly string[] DateFormats =
    [
        "dd-MM-yyyy", "d-M-yyyy",
        "dd/MM/yyyy", "d/M/yyyy",
        "yyyy-MM-dd", "yyyy-M-d",
        "MM/dd/yyyy", "M/d/yyyy",
        "dd-MM-yyyy HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss"
    ];

    public ForecastingService(
        InsightFlow.Core.Services.IDatasetService datasetService,
        IForecastingClient forecastingClient, IForecastRepository forecastRepository)
    {
        _datasetService = datasetService;
        _forecastingClient = forecastingClient;
        _forecastRepository = forecastRepository;
    }

    public async Task<ForecastResponse> GenerateForecastAsync(
        Guid datasetId,
        string userId,
        string? dateColumn = null,
        string? targetColumn = null,
        int horizon = 6,
        string? frequency = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (horizon < 1 || horizon > 365)
            throw new ArgumentOutOfRangeException(
                nameof(horizon), "Horizon must be between 1 and 365.");

        var dataset = await _datasetService.GetDatasetAsync(datasetId, userId);

        if (dataset is null)
            throw new KeyNotFoundException(
                "Dataset not found or does not belong to this user.");

        if (!string.Equals(dataset.Status, "Ready",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "The dataset must be processed before forecasting is available.");

        var metadata = await _datasetService.GetDatasetColumnsAsync(
            datasetId, userId);

        var data = await _datasetService.GetDatasetRowsAsync(
            datasetId, userId);

        if (data.Rows.Count == 0)
            throw new ArgumentException("The dataset contains no data rows.");

        var headers = data.Columns.ToList();

        int FindColumn(string name) => headers.FindIndex(
            h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

        bool TryParseDate(string text, out DateTime date)
        {
            return DateTime.TryParseExact(
                       text.Trim(), DateFormats,
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out date)
                   || DateTime.TryParse(
                       text, CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out date);
        }

        bool TryParseNumber(string text, out double value)
        {
            return double.TryParse(
                       text, NumberStyles.Number,
                       CultureInfo.InvariantCulture, out value)
                   && double.IsFinite(value);
        }

        // Detect the date column from metadata and actual row values.
        var dateCandidates = headers
            .Select((name, index) => new
            {
                Name = name,
                Index = index,
                Metadata = metadata.FirstOrDefault(c =>
                    string.Equals(c.ColumnName, name,
                        StringComparison.OrdinalIgnoreCase))
            })
            .Select(candidate =>
            {
                var sample = data.Rows
                    .Take(100)
                    .Select(row => row[candidate.Index])
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();

                var parsed = sample.Count == 0 ? 0 :
                    sample.Count(value => TryParseDate(value, out _));

                var score = sample.Count == 0 ? 0.0 :
                    (double)parsed / sample.Count;

                if (string.Equals(candidate.Metadata?.DataType, "DateTime",
                        StringComparison.OrdinalIgnoreCase))
                    score += 0.5;

                var name = candidate.Name.ToLowerInvariant();
                if (name.Contains("date") || name.Contains("timestamp"))
                    score += 0.5;

                return new
                {
                    candidate.Name,
                    candidate.Index,
                    Score = score
                };
            })
            .Where(candidate => candidate.Score > 0.5)
            .OrderByDescending(candidate => candidate.Score)
            .ToList();

        var chosenDate = string.IsNullOrWhiteSpace(dateColumn)
            ? dateCandidates.FirstOrDefault()
            : dateCandidates.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, dateColumn,
                    StringComparison.OrdinalIgnoreCase));

        if (chosenDate is null)
            throw new ArgumentException(
                "No usable date column was detected. Check the date values or specify a date column.");

        // Score numeric columns using names and metadata.
        var targetCandidates = headers
            .Select((name, index) => new
            {
                Name = name,
                Index = index,
                Metadata = metadata.FirstOrDefault(c =>
                    string.Equals(c.ColumnName, name,
                        StringComparison.OrdinalIgnoreCase))
            })
            .Where(candidate => candidate.Index != chosenDate.Index)
            .Select(candidate =>
            {
                var sample = data.Rows
                    .Take(100)
                    .Select(row => row[candidate.Index])
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();

                var numericCount = sample.Count(value =>
                    TryParseNumber(value, out _));

                var numericRatio = sample.Count == 0 ? 0.0 :
                    (double)numericCount / sample.Count;

                var name = candidate.Name.ToLowerInvariant();
                var score = numericRatio;

                if (name.Contains("revenue")) score += 10;
                else if (name.Contains("sales")) score += 9;
                else if (name.Contains("amount")) score += 8;
                else if (name.Contains("units") || name.Contains("quantity"))
                    score += 7;
                else if (name.Contains("price")) score += 4;

                if (string.Equals(candidate.Metadata?.DataType, "Integer",
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.Metadata?.DataType, "Decimal",
                        StringComparison.OrdinalIgnoreCase))
                    score += 1;

                return new
                {
                    candidate.Name,
                    candidate.Index,
                    Score = numericRatio >= 0.8 ? score : -1.0
                };
            })
            .Where(candidate => candidate.Score >= 0)
            .OrderByDescending(candidate => candidate.Score)
            .ToList();

        var chosenTarget = string.IsNullOrWhiteSpace(targetColumn)
            ? targetCandidates.FirstOrDefault()
            : targetCandidates.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, targetColumn,
                    StringComparison.OrdinalIgnoreCase));

        if (chosenTarget is null)
            throw new ArgumentException(
                "No suitable numeric target column was detected. Specify a numeric target such as Revenue.");

        var observations = new List<(DateTime Date, double Value)>();

        foreach (var row in data.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dateText = row[chosenDate.Index];
            var valueText = row[chosenTarget.Index];

            if (string.IsNullOrWhiteSpace(dateText)
                || string.IsNullOrWhiteSpace(valueText))
                continue;

            if (!TryParseDate(dateText, out var parsedDate))
                throw new ArgumentException(
                    $"Invalid date '{dateText}' in column '{chosenDate.Name}'.");

            if (!TryParseNumber(valueText, out var parsedValue))
                throw new ArgumentException(
                    $"Invalid numeric value '{valueText}' in column '{chosenTarget.Name}'.");

            if (parsedValue < 0)
                throw new ArgumentException(
                    "Negative target values are not supported by the current Python forecasting service.");

            observations.Add((parsedDate.Date, parsedValue));
        }

        if (observations.Count < 10)
            throw new ArgumentException(
                "At least 10 valid date and target observations are required.");

        observations = observations
            .GroupBy(item => item.Date)
            .Select(group => (Date: group.Key, Value: group.Sum(item => item.Value)))
            .OrderBy(item => item.Date)
            .ToList();

        if (observations.Count < 10)
            throw new ArgumentException(
                "At least 10 distinct dates are required after aggregating transactions.");

        var differences = observations
            .Zip(observations.Skip(1),
                (a, b) => (b.Date - a.Date).TotalDays)
            .ToList();

        var medianGap = differences.OrderBy(value => value).ElementAt(
            differences.Count / 2);

        var inferredFrequency = medianGap <= 1.5 ? "Daily"
            : medianGap <= 10 ? "Weekly"
            : medianGap <= 45 ? "Monthly"
            : medianGap <= 120 ? "Quarterly"
            : "Yearly";

        var selectedFrequency = string.IsNullOrWhiteSpace(frequency)
            ? inferredFrequency
            : new[] { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly" }
                .FirstOrDefault(value => string.Equals(
                    value, frequency, StringComparison.OrdinalIgnoreCase))
              ?? throw new ArgumentException("Unsupported forecast frequency.");


        var forecastRun = new InsightFlow.Core.Models.ForecastRun
        {
            DatasetId = datasetId,
            UserId = userId,
            DateColumn = chosenDate.Name,
            TargetColumn = chosenTarget.Name,
            Frequency = selectedFrequency,
            Horizon = horizon,
            Status = "Running",
            CreatedAtUtc = DateTime.UtcNow,
            StartedAtUtc = DateTime.UtcNow,
            ConfigurationJson = JsonSerializer.Serialize(new
            {
                DateColumn = chosenDate.Name,
                TargetColumn = chosenTarget.Name,
                Frequency = selectedFrequency,
                Horizon = horizon
            })
        };

        // Save the run before calling Python.
        await _forecastRepository.AddAsync(forecastRun);

        ForecastResponse result;

        try
        {
            result = await _forecastingClient.GenerateForecastAsync(
                new ForecastRequest
                {
                    Dates = observations.Select(item =>
                        item.Date.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture)).ToList(),

                    Values = observations.Select(item => item.Value).ToList(),
                    Horizon = horizon,
                    Frequency = selectedFrequency
                },
                cancellationToken);
        }
        catch
        {
            forecastRun.Status = "Failed";
            forecastRun.CompletedAtUtc = DateTime.UtcNow;

            // Preserve the failure status before propagating the error.
            await _forecastRepository.UpdateAsync(forecastRun);

            throw;
        }

        // Store model and evaluation results.
        forecastRun.ModelVersion = result.ModelUsed;
        forecastRun.MetricsJson = JsonSerializer.Serialize(result.Evaluation);
        forecastRun.Status = "Completed";
        forecastRun.CompletedAtUtc = DateTime.UtcNow;

        // Store each forecast date and prediction.
        foreach (var prediction in result.Forecasts)
        {
            forecastRun.ForecastPoints.Add(
                new InsightFlow.Core.Models.ForecastPoint
                {
                    ForecastDate = prediction.Date,
                    PredictedValue = Convert.ToDecimal(prediction.Predicted)
                });
        }

        // Save the completed run and its related forecast points.
        await _forecastRepository.UpdateAsync(forecastRun);

        return result;

    }
}
