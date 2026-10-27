using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class Conversation
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public Guid? DatasetId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public virtual ICollection<AgentToolExecution> AgentToolExecutions { get; set; } = new List<AgentToolExecution>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual Dataset? Dataset { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
