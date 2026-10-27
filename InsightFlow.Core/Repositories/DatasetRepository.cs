using System;
using System.Collections.Generic;
using System.Text;
using InsightFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InsightFlow.Core.Repositories
{
    public class DatasetRepository:IDatasetRepository
    {
        private readonly InsightFlowAiContext _context;

        public DatasetRepository(InsightFlowAiContext context)
        {
            _context = context;
        }

        public async Task<Dataset?> GetByIdAsync(Guid id, string userId)
        {
            return await _context.Datasets
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        }

        public async Task<IReadOnlyList<Dataset>> GetByUserIdAsync(string userId)
        {
            return await _context.Datasets
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task AddAsync(Dataset dataset)
        {
            await _context.Datasets.AddAsync(dataset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Dataset dataset)
        {
            _context.Datasets.Update(dataset);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var dataset = await _context.Datasets
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (dataset is null)
            {
                return false;
            }

            _context.Datasets.Remove(dataset);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ReplaceColumnsAsync(
        Guid datasetId,
        string userId,
        IReadOnlyList<DatasetColumn> columns)
        {
            ArgumentNullException.ThrowIfNull(columns);

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var datasetExists = await _context.Datasets
                .AnyAsync(d => d.Id == datasetId && d.UserId == userId);

            if (!datasetExists)
            {
                throw new KeyNotFoundException(
                    "Dataset not found or does not belong to this user.");
            }

            var existingColumns = await _context.DatasetColumns
                .Where(c => c.DatasetId == datasetId)
                .ToListAsync();

            _context.DatasetColumns.RemoveRange(existingColumns);

            foreach (var column in columns)
            {
                column.Id = 0;
                column.DatasetId = datasetId;
            }

            await _context.DatasetColumns.AddRangeAsync(columns);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public async Task<IReadOnlyList<DatasetColumn>> GetColumnsByDatasetIdAsync(
            Guid datasetId,
            string userId)
        {
            return await _context.DatasetColumns
                .Where(column =>
                    column.DatasetId == datasetId &&
                    column.Dataset.UserId == userId)
                .OrderBy(column => column.OrdinalPosition)
                .ToListAsync();
        }

    }
}
