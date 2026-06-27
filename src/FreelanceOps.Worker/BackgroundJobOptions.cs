namespace FreelanceOps.Worker;

public sealed class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    public int DueDateMonitoringIntervalMinutes { get; init; } = 60;
}
