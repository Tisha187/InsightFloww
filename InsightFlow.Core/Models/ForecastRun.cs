using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class ForecastRun
{
    public Guid Id { get; set; }

    public Guid DatasetId { get; set; }

    public string UserId { get; set; } = null!;

    public string TargetColumn { get; set; } = null!;

    public string DateColumn { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public int Horizon { get; set; }

    public string? ModelVersion { get; set; }

    public string? ConfigurationJson { get; set; }

    public string? MetricsJson { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public virtual Dataset Dataset { get; set; } = null!;

    public virtual ICollection<ForecastPoint> ForecastPoints { get; set; } = new List<ForecastPoint>();

    public virtual AspNetUser User { get; set; } = null!;
}
