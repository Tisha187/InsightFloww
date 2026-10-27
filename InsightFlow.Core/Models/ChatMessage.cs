using System;
using System.Collections.Generic;

namespace InsightFlow.Core.Models;

public partial class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? MetadataJson { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<AgentToolExecution> AgentToolExecutions { get; set; } = new List<AgentToolExecution>();

    public virtual Conversation Conversation { get; set; } = null!;
}
