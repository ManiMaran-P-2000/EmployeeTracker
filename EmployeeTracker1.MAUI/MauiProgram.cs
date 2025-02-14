using EmployeeTracker1.MAUI.Services;
using EmployeeTracker1.MAUI.ViewModels;
using EmployeeTracker1.MAUI.Views;
using Microsoft.Extensions.Logging;

namespace EmployeeTracker1.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<UnlockReasonViewModel>();

            // Register service
            builder.Services.AddSingleton<WebSocketServerService>(); 

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddSingleton<UnlockReasonPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            var webSocketServer = app.Services.GetRequiredService<WebSocketServerService>();
            Task.Run(() => webSocketServer.StartServer());

            return app;
        }
    }
}
