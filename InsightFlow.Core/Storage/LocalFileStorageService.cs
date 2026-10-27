
using Microsoft.Extensions.Configuration;
using InsightFlow.Core.Storage;

namespace InsightFlow.Core.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:RootPath"];

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException(
                "FileStorage:RootPath is not configured.");
        }

        _rootPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                configuredPath));

        Directory.CreateDirectory(_rootPath);
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
            fileExtension.Skip(1).Any(c => !char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException(
                "A valid file extension is required.",
                nameof(fileExtension));
        }

        var storageKey = $"{Guid.NewGuid():N}{fileExtension.ToLowerInvariant()}";
        var fullPath = GetFullPath(storageKey);

        await using var destination = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            options: FileOptions.Asynchronous);

        await fileStream.CopyToAsync(destination, cancellationToken);

        return storageKey;
    }

    public Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = GetFullPath(storageKey);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous);

        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = GetFullPath(storageKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            storageKey.Length > 100 ||
            storageKey != Path.GetFileName(storageKey) ||
            storageKey.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            storageKey.Contains('/') ||
            storageKey.Contains('\\') ||
            storageKey.Contains(':') ||
            storageKey.Contains(".."))
        {
            throw new ArgumentException(
                "Invalid storage key.",
                nameof(storageKey));
        }

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, storageKey));

        var rootWithSeparator =
            _rootPath.TrimEnd(Path.DirectorySeparatorChar) +
            Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Storage key resolves outside the storage directory.",
                nameof(storageKey));
        }

        return fullPath;
    }
}
