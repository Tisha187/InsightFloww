
using InsightFlow.Core.Analytics;
using InsightFlow.Core.Models;
using InsightFlow.Core.Repositories;
using InsightFlow.Core.Storage;

namespace InsightFlow.Core.Services;

public class DatasetService : IDatasetService
{
    private readonly IDatasetRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly DatasetReader _datasetReader;
    private readonly DatasetAnalyticsService _analyticsService;

    public DatasetService(
        IDatasetRepository repository,
        IFileStorageService fileStorage,
        DatasetReader datasetReader,
        DatasetAnalyticsService analyticsService)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _datasetReader = datasetReader;
        _analyticsService = analyticsService;
    }

    public async Task<Dataset?> GetDatasetAsync(
        Guid id, string userId)
    {
        ValidateUserId(userId);
        return await _repository.GetByIdAsync(id, userId);
    }

    public async Task<IReadOnlyList<Dataset>> GetUserDatasetsAsync(
        string userId)
    {
        ValidateUserId(userId);
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<Dataset> CreateDatasetAsync(
        Dataset dataset, string userId)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        ValidateUserId(userId);

        dataset.UserId = userId;

        await _repository.AddAsync(dataset);

        return dataset;
    }

    public async Task<bool> DeleteDatasetAsync(
    Guid datasetId,
    string userId)
    {
        ValidateUserId(userId);

        // Verify ownership and retrieve the storage key.
        var dataset = await _repository.GetByIdAsync(
            datasetId,
            userId);

        if (dataset is null)
            return false;

        var storageKey = dataset.StorageKey;

        // Delete the database record.
        var deleted = await _repository.DeleteAsync(
            datasetId,
            userId);

        if (!deleted)
            return false;

        // Remove the physical file after the database deletion succeeds.
        try
        {
            await _fileStorage.DeleteAsync(
                storageKey,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "The dataset record was deleted, but its stored file could not be removed. File cleanup must be retried.",
                ex);
        }

        return true;
    }

    public async Task ProcessDatasetAsync(
        Guid datasetId, string userId)
    {
        ValidateUserId(userId);

        var dataset = await _repository.GetByIdAsync(
            datasetId, userId);

        if (dataset is null)
            throw new KeyNotFoundException(
                "Dataset not found or does not belong to this user.");

        await using var stream = await _fileStorage.OpenReadAsync(
            dataset.StorageKey);

        if (stream is null)
            throw new FileNotFoundException(
                "The stored dataset file could not be found.");

        var result = await _datasetReader.ReadAsync(
            stream, dataset.FileFormat);

        dataset.RowCount = result.RowCount;
        dataset.Status = "Ready";

        await _repository.UpdateAsync(dataset);


        var columns = result.Columns.Select(column => new DatasetColumn
        {
            DatasetId = dataset.Id,
            ColumnName = column.ColumnName,
            DataType = column.DataType,
            SemanticRole = column.SemanticRole,
            OrdinalPosition = column.OrdinalPosition,
            IsNullable = column.IsNullable,
            MissingCount = column.MissingCount,
            MissingPercentage = column.MissingPercentage,
            DistinctCount = column.DistinctCount
        }).ToList();


        await _repository.ReplaceColumnsAsync(
            dataset.Id, userId, columns);
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User ID is required.", nameof(userId));
    }


    public async Task<IReadOnlyList<DatasetColumn>> GetDatasetColumnsAsync(
    Guid datasetId,
    string userId)
    {
        ValidateUserId(userId);

        var dataset = await _repository.GetByIdAsync(datasetId, userId);

        if (dataset is null)
            throw new KeyNotFoundException(
                "Dataset not found or does not belong to this user.");

        return await _repository.GetColumnsByDatasetIdAsync(datasetId, userId);
    }


    public async Task<DatasetPreviewResult> GetDatasetPreviewAsync(
        Guid datasetId,
        string userId,
        int maxRows = 10)
    {
        ValidateUserId(userId);

        var dataset = await _repository.GetByIdAsync(datasetId, userId);

        if (dataset is null)
            throw new KeyNotFoundException(
                "Dataset not found or does not belong to this user.");

        if (!string.Equals(dataset.Status, "Ready",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The dataset must be processed before previewing it.");
        }

        await using var stream = await _fileStorage.OpenReadAsync(
            dataset.StorageKey);

        if (stream is null)
            throw new FileNotFoundException(
                "The stored dataset file could not be found.");

        return await _datasetReader.ReadPreviewAsync(
            stream,
            dataset.FileFormat,
            maxRows);
    }



    public async Task<DatasetSummaryResult> GetDatasetSummaryAsync(
        Guid datasetId,
        string userId)
    {
        ValidateUserId(userId);

        var dataset = await _repository.GetByIdAsync(datasetId, userId);

        if (dataset is null)
        {
            throw new KeyNotFoundException(
                "Dataset not found or does not belong to this user.");
        }

        if (!string.Equals(
            dataset.Status,
            "Ready",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The dataset must be processed before analytics are available.");
        }

        await using var stream = await _fileStorage.OpenReadAsync(
            dataset.StorageKey);

        if (stream is null)
        {
            throw new FileNotFoundException(
                "The stored dataset file could not be found.");
        }

        var data = await _datasetReader.ReadAllRowsAsync(
            stream,
            dataset.FileFormat);

        var columnMetadata = await _repository.GetColumnsByDatasetIdAsync(
            datasetId,
            userId);

        // The metadata is retrieved from the existing Database First schema.
        // Pass it to the analytics service in the next integration step.
        return _analyticsService.CalculateSummary(
            dataset.Id,
            data.Columns,
            data.Rows);
    }



}
