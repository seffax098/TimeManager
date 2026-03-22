namespace Backend.Models;

public enum UserRole
{
    employee = 1,
    admin = 2
}

public enum SessionStatus
{
    active = 1,
    paused = 2,
    completed = 3
}

public enum ActivityVerdict
{
    work = 1,
    rest = 2,
    unknown = 3
}

public enum ReportStatusColor
{
    green = 1,
    yellow = 2,
    red = 3
}