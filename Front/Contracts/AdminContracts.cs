using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Front.Contracts
{
    public class AdminEmployeeDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public decimal WorkPercent { get; set; }
        public decimal RestPercent { get; set; }
        public string StatusColor { get; set; }
        public List<SiteReportDto> Sites { get; set; }
    }

    public class AdminEmployeesResponse
    {
        public DateOnly Date { get; set; }
        public List<AdminEmployeeDto> Employees { get; set; }
    }

    public class AdminEmployeeDetailsResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public DateOnly Date { get; set; }
        public decimal WorkPercent { get; set; }
        public decimal RestPercent { get; set; }
        public string StatusColor { get; set; }
        public List<SiteReportDto> Sites { get; set; }
    }
}