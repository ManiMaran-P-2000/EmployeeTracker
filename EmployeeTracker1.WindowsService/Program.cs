using EmployeeTracker1.WindowsService;
using EmployeeTracker1.WindowsService.Services;
using EmployeeTracker1.WindowsService.Services.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IIdleTracker, WindowsIdleTracker>();
builder.Services.AddSignalR();
builder.Services.AddLogging(logging => logging.AddConsole());

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SignalRHub>("/trackerHub");
});

var idleTracker = app.Services.GetRequiredService<IIdleTracker>();
await app.StartAsync();

Console.WriteLine("Background process running. Press Ctrl+C to exit.");
await Task.Delay(Timeout.Infinite);

idleTracker.DisableTracking();
await app.StopAsync();
