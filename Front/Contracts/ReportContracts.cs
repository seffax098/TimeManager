using System;
using System.Collections.Generic;

namespace Front.Contracts
{
    public class UrlVisitDto
    {
        public string Url { get; set; }
        public int DurationSec { get; set; }
        public string Verdict { get; set; }
    }

    public class SiteReportDto
    {
        public string Domain { get; set; }
        public int DurationSec { get; set; }
        public string Verdict { get; set; }
        public List<UrlVisitDto> Urls { get; set; }
    }

    public class DayReportResponse
    {
        public DateOnly Date { get; set; }
        public decimal WorkPercent { get; set; }
        public decimal RestPercent { get; set; }
        public string StatusColor { get; set; }
        public List<SiteReportDto> Sites { get; set; }
    }
}