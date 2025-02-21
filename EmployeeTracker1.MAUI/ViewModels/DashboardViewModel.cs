using EmployeeTracker1.MAUI.Helpers;
using EmployeeTracker1.MAUI.Models;
using EmployeeTracker1.MAUI.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Windows.Input;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly SignalRClientService _signalRService;
        private readonly BackgroundProcessService _backgroundService;
        public ICommand StartTrackingCommand { get; }
        public ICommand StopTrackingCommand { get; }

        public string WelcomeMessage { get; private set; } = "Welcome, User!";

        public DashboardViewModel(SignalRClientService signalRService, BackgroundProcessService backgroundService)
        {
            _signalRService = signalRService;
            _backgroundService = backgroundService;

            StartTrackingCommand = new Command(async () => await StartTracking());
            StopTrackingCommand = new Command(async () => await StopTracking());

            _signalRService.OnSystemUnlocked += async () => await Shell.Current.GoToAsync("///UnlockReasonPage");
            Task.Run(async () => await _signalRService.StartAsync());
        }

        private void OnReasonSubmitted(UnlockReasonData reason)
        {

        }

        private async Task StartTracking()
        {
            try
            {
                _backgroundService.StartBackgroundProcess();
                await Task.Delay(1000);
                await _signalRService.SendCommand("START");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to start tracking: {ex.Message}", "OK");
            }
        }

        private async Task StopTracking()
        {
            try
            {
                await _signalRService.SendCommand("STOP");
                _backgroundService.StopBackgroundProcess();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to stop tracking: {ex.Message}", "OK");
            }
        }
    }
}
