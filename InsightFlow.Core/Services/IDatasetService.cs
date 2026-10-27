using InsightFlow.Core.Analytics;
using InsightFlow.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsightFlow.Core.Services
{
    public interface IDatasetService
    {
        Task<Dataset?> GetDatasetAsync(Guid id, string userId);

        Task<IReadOnlyList<Dataset>> GetUserDatasetsAsync(
            string userId);

        Task<Dataset> CreateDatasetAsync(
        Dataset dataset,
        string userId);

        Task ProcessDatasetAsync(Guid datasetId, string userId);

        Task<IReadOnlyList<DatasetColumn>> GetDatasetColumnsAsync(
        Guid datasetId,
        string userId);

        Task<DatasetPreviewResult> GetDatasetPreviewAsync(
        Guid datasetId,
        string userId,
        int maxRows = 10);

        Task<DatasetSummaryResult> GetDatasetSummaryAsync(
        Guid datasetId,
        string userId);

        Task<bool> DeleteDatasetAsync(Guid datasetId, string userId);


    }
}
