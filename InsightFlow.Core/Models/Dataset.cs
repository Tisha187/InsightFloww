using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class Dataset
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string FileFormat { get; set; } = null!;

    public string StorageKey { get; set; } = null!;

    public long? RowCount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<DatasetColumn> DatasetColumns { get; set; } = new List<DatasetColumn>();

    public virtual ICollection<ForecastRun> ForecastRuns { get; set; } = new List<ForecastRun>();

    public virtual AspNetUser User { get; set; } = null!;
}
