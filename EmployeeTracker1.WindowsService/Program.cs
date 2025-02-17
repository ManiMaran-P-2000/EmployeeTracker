using EmployeeTracker1.WindowsService;
using EmployeeTracker1.WindowsService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IdleTrackerService>();
        services.AddSignalR();
        services.AddHostedService<IdleTrackerService>();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseUrls("http://localhost:5000");
        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SignalRHub>("/trackerHub");
            });
        });
    });

var host = builder.Build();
host.Run();
