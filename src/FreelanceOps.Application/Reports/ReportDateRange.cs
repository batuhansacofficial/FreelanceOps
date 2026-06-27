namespace FreelanceOps.Application.Reports;

internal sealed record ResolvedReportDateRange(DateOnly From, DateOnly To)
{
    public DateTime FromUtc =>
        From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    public DateTime ToExclusiveUtc =>
        To == DateOnly.MaxValue
            ? DateTime.MaxValue
            : To.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}

internal static class ReportDateRange
{
    public static ResolvedReportDateRange Resolve(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resolvedFrom = from ?? new DateOnly(today.Year, today.Month, 1);
        var resolvedTo = to ?? today;

        return new ResolvedReportDateRange(resolvedFrom, resolvedTo);
    }

    public static bool IsOrdered(DateOnly? from, DateOnly? to)
    {
        var range = Resolve(from, to);

        return range.From <= range.To;
    }

    public static bool IsWithinMaximumRange(DateOnly? from, DateOnly? to)
    {
        var range = Resolve(from, to);

        return range.From <= range.To &&
               range.To.DayNumber - range.From.DayNumber <= 365;
    }
}
