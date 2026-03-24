using System;

namespace Front.Contracts
{
    public class ActivityRequest
    {
        public Guid SessionId { get; set; }
        public string Domain { get; set; }
        public string Url { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset EndedAt { get; set; }
        public int DurationSec { get; set; }
        public string? Verdict { get; set; }
    }

    public class ActivityResponse
    {
        public Guid ActivityId { get; set; }
        public Guid SessionId { get; set; }
        public string Domain { get; set; }
        public string Url { get; set; }
        public int DurationSec { get; set; }
        public string Verdict { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}