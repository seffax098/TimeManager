using System;

namespace Front.Contracts
{
    public class CreateViolationRequest
    {
        public Guid ActivityId { get; set; }
        public string Reason { get; set; }
        // For multipart request the file will be added separately as form data
    }

    public class ViolationResponse
    {
        public Guid ViolationId { get; set; }
        public Guid ActivityId { get; set; }
        public string Reason { get; set; }
        public string ScreenshotPath { get; set; }
        public bool IsDisputed { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}