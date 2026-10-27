
namespace InsightFlow.Api.DTOs;

public sealed class DatasetUploadResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string OriginalFileName { get; init; } = string.Empty;

    public string FileFormat { get; init; } = string.Empty;

    public long? RowCount { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }
}
