using InsightFlow.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsightFlow.Core.Repositories
{
    public interface IDatasetRepository
    {
        Task<Dataset?> GetByIdAsync(Guid id, string userId);

        Task<IReadOnlyList<Dataset>> GetByUserIdAsync(string userId);

        Task AddAsync(Dataset dataset);

        Task UpdateAsync(Dataset dataset);

        Task<bool> DeleteAsync(Guid id, string userId);

        Task ReplaceColumnsAsync(
        Guid datasetId,
        string userId,
       IReadOnlyList<DatasetColumn> columns);

       Task<IReadOnlyList<DatasetColumn>> GetColumnsByDatasetIdAsync(
        Guid datasetId,
        string userId);
    }
}
