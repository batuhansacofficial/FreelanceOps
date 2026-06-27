using FreelanceOps.Application;
using FreelanceOps.Infrastructure;
using FreelanceOps.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<BackgroundJobOptions>(
    builder.Configuration.GetSection(BackgroundJobOptions.SectionName));
builder.Services.AddHostedService<DueDateMonitoringWorker>();

var host = builder.Build();
host.Run();
