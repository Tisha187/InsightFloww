
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsightFlow.Core.Models;

namespace InsightFlow.Core.Repositories;

public interface IForecastRepository
{
    Task AddAsync(ForecastRun forecastRun);

    Task UpdateAsync(ForecastRun forecastRun);

    Task<IReadOnlyList<ForecastRun>> GetHistoryAsync(
        Guid datasetId,
        string userId);

    Task<ForecastRun?> GetByIdAsync(
        Guid runId,
        string userId);
}
