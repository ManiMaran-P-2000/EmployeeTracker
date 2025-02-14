using EmployeeTracker1.WindowsService;
using EmployeeTracker1.WindowsService.Services;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<IdleTrackerService>();
    });

var host = builder.Build();
host.Run();
