using System;

namespace Front.Contracts
{
    public class StartTimerRequest
    {
        public DateOnly? Date { get; set; }
    }

    public class StartTimerResponse
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public DateOnly WorkDate { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public string Status { get; set; }
    }

    public class StopTimerRequest
    {
        public Guid SessionId { get; set; }
    }

    public class StopTimerResponse
    {
        public Guid SessionId { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset EndedAt { get; set; }
        public int TotalSeconds { get; set; }
        public int WorkTime { get; set; }
        public int RestTime { get; set; }
        public string Status { get; set; }
    }
}