using EmployeeTracker1.MAUI.Services;
using EmployeeTracker1.MAUI.Services.Interface;
using EmployeeTracker1.MAUI.ViewModels;
using EmployeeTracker1.MAUI.Views;
using Microsoft.Extensions.Logging;
#if WINDOWS
using EmployeeTracker1.MAUI.Platforms.Windows.Services;
#endif
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

            builder.Services.AddSingleton<SignalRClientService>();
            builder.Services.AddSingleton<BackgroundProcessService>();

#if WINDOWS
            builder.Services.AddSingleton<IWindowRestrictionService, WindowsRestrictionService>();
#else
            builder.Services.AddSingleton<IWindowRestrictionService>(provider =>
                new NullRestrictionService(provider.GetRequiredService<ILogger<NullRestrictionService>>()));
#endif

            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<UnlockReasonViewModel>();

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddSingleton<UnlockReasonPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    public class NullRestrictionService : IWindowRestrictionService
    {
        private readonly ILogger<NullRestrictionService> _logger;

        public NullRestrictionService(ILogger<NullRestrictionService> logger)
        {
            _logger = logger;
        }

        public void RestrictWindow() => _logger.LogWarning("Window restriction not supported.");
        public void RestoreWindow() => _logger.LogWarning("Window restoration not supported.");
    }
}
