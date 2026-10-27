using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class AgentToolExecution
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid? ChatMessageId { get; set; }

    public string ToolName { get; set; } = null!;

    public string? ArgumentsJson { get; set; }

    public string? ResultSummary { get; set; }

    public string Status { get; set; } = null!;

    public long? DurationMs { get; set; }

    public string? ErrorCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public virtual ChatMessage? ChatMessage { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
