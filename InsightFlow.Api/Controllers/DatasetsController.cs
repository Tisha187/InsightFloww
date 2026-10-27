
using System.Security.Claims;
using InsightFlow.Api.DTOs;
using InsightFlow.Core.Models;
using InsightFlow.Core.Services;
using InsightFlow.Core.Storage;
using InsightFlow.Core.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DatasetsController : ControllerBase
{
    private readonly IDatasetService _datasetService;
    private readonly IFileStorageService _fileStorage;
    private readonly DatasetFileValidator _fileValidator;
    private readonly IConfiguration _configuration;

    public DatasetsController(
        IDatasetService datasetService,
        IFileStorageService fileStorage,
        DatasetFileValidator fileValidator,
        IConfiguration configuration)
    {
        _datasetService = datasetService;
        _fileStorage = fileStorage;
        _fileValidator = fileValidator;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var datasets = await _datasetService.GetUserDatasetsAsync(userId);
        return Ok(datasets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dataset = await _datasetService.GetDatasetAsync(id, userId);

        if (dataset is null)
            return NotFound();

        return Ok(dataset);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] DatasetUploadRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (request.File is null || request.File.Length == 0)
            return BadRequest("Please select a non-empty CSV or XLSX file.");

        var maxSize = _configuration.GetValue<long>(
            "FileStorage:MaxFileSizeBytes");

        if (maxSize <= 0)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "File upload size is not configured correctly.");

        if (request.File.Length > maxSize)
            return BadRequest($"The file exceeds the {maxSize}-byte size limit.");

        var originalFileName = Path.GetFileName(request.File.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (extension is not ".csv" and not ".xlsx")
            return BadRequest("Only CSV and XLSX files are supported.");

        var displayName = string.IsNullOrWhiteSpace(request.Name)
            ? Path.GetFileNameWithoutExtension(originalFileName)
            : request.Name.Trim();

        if (displayName.Length > 200)
            return BadRequest("Dataset name cannot exceed 200 characters.");

        string? storageKey = null;

        try
        {
            await using var input = request.File.OpenReadStream();

            await _fileValidator.ValidateAsync(
                input,
                originalFileName,
                maxSize,
                cancellationToken);

            storageKey = await _fileStorage.SaveAsync(
                input,
                extension,
                cancellationToken);

            var dataset = new Dataset
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = displayName,
                OriginalFileName = originalFileName,
                FileFormat = extension.TrimStart('.').ToUpperInvariant(),
                StorageKey = storageKey,
                RowCount = null,
                Status = "Uploaded",
                CreatedAtUtc = DateTime.UtcNow
            };

            await _datasetService.CreateDatasetAsync(dataset, userId);

            var response = new DatasetUploadResponse
            {
                Id = dataset.Id,
                Name = dataset.Name,
                OriginalFileName = dataset.OriginalFileName,
                FileFormat = dataset.FileFormat,
                RowCount = dataset.RowCount,
                Status = dataset.Status,
                CreatedAtUtc = dataset.CreatedAtUtc
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = dataset.Id },
                response);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            // Avoid leaving an orphaned file if the database save fails.
            if (!string.IsNullOrWhiteSpace(storageKey))
            {
                try
                {
                    await _fileStorage.DeleteAsync(
                        storageKey,
                        cancellationToken);
                }
                catch
                {
                    // Preserve the original failure.
                }
            }

            throw;
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }


    [HttpPost("{id:guid}/process")]
    public async Task<IActionResult> Process(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            await _datasetService.ProcessDatasetAsync(id, userId);

            var dataset = await _datasetService.GetDatasetAsync(id, userId);

            if (dataset is null)
                return NotFound();

            return Ok(new
            {
                dataset.Id,
                dataset.Name,
                dataset.RowCount,
                dataset.Status
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FileNotFoundException)
        {
            return NotFound("The stored dataset file could not be found.");
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("{id:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var columns = await _datasetService.GetDatasetColumnsAsync(
                id, userId);

            var response = columns.Select(column => new
            {
                column.ColumnName,
                column.DataType,
                column.SemanticRole,
                column.OrdinalPosition,
                column.IsNullable,
                column.MissingCount,
                column.MissingPercentage,
                column.DistinctCount
            });

            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> GetPreview(
        Guid id,
        [FromQuery] int rows = 10)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (rows < 1 || rows > 100)
            return BadRequest("Preview rows must be between 1 and 100.");

        try
        {
            var preview = await _datasetService.GetDatasetPreviewAsync(
                id, userId, rows);

            return Ok(preview);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FileNotFoundException)
        {
            return NotFound("The stored dataset file could not be found.");
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetDatasetSummary(Guid id)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var summary = await _datasetService.GetDatasetSummaryAsync(
                id,
                userId);

            return Ok(summary);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Dataset not found."
            });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new
            {
                message = "The stored dataset file could not be found."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


}
