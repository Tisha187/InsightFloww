
namespace InsightFlow.Core.Models;

public partial class DatasetFile
{
    public string StorageKey { get; set; } = null!;

    public byte[] FileContent { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}
