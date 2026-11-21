
using InsightFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InsightFlow.Core.Storage;

public sealed class SqlFileStorageService : IFileStorageService
{
    private readonly InsightFlowAiContext _context;

    public SqlFileStorageService(InsightFlowAiContext context)
    {
        _context = context;
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        if (!fileStream.CanRead)
        {
            throw new ArgumentException(
                "The supplied stream must be readable.",
                nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileExtension) ||
            fileExtension.Length > 10 ||
            fileExtension[0] != '.' ||
            fileExtension.Skip(1).Any(
                c => !char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException(
                "A valid file extension is required.",
                nameof(fileExtension));
        }

        var storageKey =
            $"{Guid.NewGuid():N}{fileExtension.ToLowerInvariant()}";

        await using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(
            memoryStream,
            cancellationToken);

        var datasetFile = new DatasetFile
        {
            StorageKey = storageKey,
            FileContent = memoryStream.ToArray()
        };

        _context.DatasetFiles.Add(datasetFile);

        await _context.SaveChangesAsync(cancellationToken);

        return storageKey;
    }

    public async Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        ValidateStorageKey(storageKey);

        var fileContent = await _context.DatasetFiles
            .AsNoTracking()
            .Where(file => file.StorageKey == storageKey)
            .Select(file => file.FileContent)
            .SingleOrDefaultAsync(cancellationToken);

        if (fileContent is null)
        {
            return null;
        }

        return new MemoryStream(fileContent, writable: false);
    }

    public async Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        ValidateStorageKey(storageKey);

        var datasetFile = await _context.DatasetFiles
            .SingleOrDefaultAsync(
                file => file.StorageKey == storageKey,
                cancellationToken);

        if (datasetFile is null)
        {
            return;
        }

        _context.DatasetFiles.Remove(datasetFile);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            storageKey.Length > 100 ||
            storageKey != Path.GetFileName(storageKey) ||
            storageKey.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0 ||
            storageKey.Contains('/') ||
            storageKey.Contains('\\') ||
            storageKey.Contains(':') ||
            storageKey.Contains(".."))
        {
            throw new ArgumentException(
                "Invalid storage key.",
                nameof(storageKey));
        }
    }
}
