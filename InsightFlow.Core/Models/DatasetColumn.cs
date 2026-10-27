using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class DatasetColumn
{
    public long Id { get; set; }

    public Guid DatasetId { get; set; }

    public string ColumnName { get; set; } = null!;

    public string DataType { get; set; } = null!;

    public string? SemanticRole { get; set; }

    public int OrdinalPosition { get; set; }

    public long MissingCount { get; set; }

    public double MissingPercentage { get; set; }

    public long? DistinctCount { get; set; }

    public bool IsNullable { get; set; }

    public virtual Dataset Dataset { get; set; } = null!;

  
}
