
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using InsightFlow.Core.Models.Forecasting;
using InsightFlow.Core.Repositories;
using InsightFlow.Core.Services.Forecasting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ForecastingController : ControllerBase
{
    private readonly ForecastingService _forecastingService;
    private readonly IForecastRepository _forecastRepository;

    public ForecastingController(
        ForecastingService forecastingService,
        IForecastRepository forecastRepository)
    {
        _forecastingService = forecastingService;
        _forecastRepository = forecastRepository;
    }

    public sealed class CreateForecastRequest
    {
        public string DateColumn { get; set; } = string.Empty;
        public string TargetColumn { get; set; } = string.Empty;
        public int Horizon { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }

    [HttpPost("{datasetId:guid}")]
    public async Task<IActionResult> CreateForecast(
        Guid datasetId,
        [FromBody] CreateForecastRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _forecastingService.GenerateForecastAsync(
                datasetId,
                userId,
                request.DateColumn,
                request.TargetColumn,
                request.Horizon,
                request.Frequency,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Dataset not found or does not belong to this user."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "The forecasting service could not process the request."
            });
        }
    }

    [HttpGet("{datasetId:guid}/history")]
    public async Task<IActionResult> GetForecastHistory(Guid datasetId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var runs = await _forecastRepository.GetHistoryAsync(
            datasetId,
            userId);

        return Ok(runs.Select(run => new
        {
            runId = run.Id,
            datasetId = run.DatasetId,
            dateColumn = run.DateColumn,
            targetColumn = run.TargetColumn,
            frequency = run.Frequency,
            horizon = run.Horizon,
            modelVersion = run.ModelVersion,
            status = run.Status,
            createdAtUtc = run.CreatedAtUtc,
            completedAtUtc = run.CompletedAtUtc
        }));
    }

    [HttpGet("runs/{runId:guid}")]
    public async Task<IActionResult> GetForecastRun(Guid runId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var run = await _forecastRepository.GetByIdAsync(
            runId,
            userId);

        if (run is null)
        {
            return NotFound(new
            {
                message = "Forecast run not found."
            });
        }

        ForecastEvaluation? metrics = null;

        if (!string.IsNullOrWhiteSpace(run.MetricsJson))
        {
            metrics = JsonSerializer.Deserialize<ForecastEvaluation>(
                run.MetricsJson,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        return Ok(new
        {
            runId = run.Id,
            datasetId = run.DatasetId,
            dateColumn = run.DateColumn,
            targetColumn = run.TargetColumn,
            frequency = run.Frequency,
            horizon = run.Horizon,
            modelVersion = run.ModelVersion,
            configuration = run.ConfigurationJson,
            metrics,
            status = run.Status,
            createdAtUtc = run.CreatedAtUtc,
            startedAtUtc = run.StartedAtUtc,
            completedAtUtc = run.CompletedAtUtc,
            forecasts = run.ForecastPoints
                .OrderBy(point => point.ForecastDate)
                .Select(point => new
                {
                    date = point.ForecastDate,
                    predicted = point.PredictedValue,
                    lowerBound = point.LowerBound,
                    upperBound = point.UpperBound
                })
        });
    }
}
