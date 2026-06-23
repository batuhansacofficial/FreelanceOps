namespace FreelanceOps.Application.Reports;

internal static class ReportMath
{
    public static double ToHours(int minutes)
    {
        return Math.Round(minutes / 60d, 2);
    }

    public static decimal RevenuePerHour(decimal paidAmount, int trackedMinutes)
    {
        if (trackedMinutes <= 0)
        {
            return 0m;
        }

        return Math.Round(paidAmount / (trackedMinutes / 60m), 2);
    }
}
