
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InsightFlow.Core.Repositories;

public sealed class ForecastRepository : IForecastRepository
{
    private readonly InsightFlowAiContext _context;

    public ForecastRepository(InsightFlowAiContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ForecastRun forecastRun)
    {
        await _context.ForecastRuns.AddAsync(forecastRun);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ForecastRun forecastRun)
    {
        _context.ForecastRuns.Update(forecastRun);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ForecastRun>> GetHistoryAsync(
        Guid datasetId,
        string userId)
    {
        return await _context.ForecastRuns
            .AsNoTracking()
            .Where(run =>
                run.DatasetId == datasetId &&
                run.UserId == userId)
            .OrderByDescending(run => run.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<ForecastRun?> GetByIdAsync(
        Guid runId,
        string userId)
    {
        return await _context.ForecastRuns
            .AsNoTracking()
            .Include(run => run.ForecastPoints)
            .FirstOrDefaultAsync(run =>
                run.Id == runId &&
                run.UserId == userId);
    }
}
