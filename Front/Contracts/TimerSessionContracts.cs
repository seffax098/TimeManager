using System;

namespace Front.Contracts;

public class ActiveTimerSessionResponse
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public string Status { get; set; }
}
