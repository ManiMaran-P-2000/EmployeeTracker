using EmployeeTracker1.MAUI.Helpers;
using EmployeeTracker1.MAUI.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Windows.Input;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class DashboardViewModel: BaseViewModel
    {
        private HubConnection _hubConnection;

        public ICommand StartTrackingCommand { get; }
        public ICommand StopTrackingCommand { get; }

        public string WelcomeMessage { get; private set; } = "Welcome, User!";

        public DashboardViewModel()
        {
            StartTrackingCommand = new Command(async () => await StartTracking());
            StopTrackingCommand = new Command(async () => await StopTracking());

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/trackerHub")
                .Build();

            _hubConnection.On("OnSystemUnlocked", ShowUnlockReasonPrompt);
            _hubConnection.On<UnlockReasonData>("ReceiveUnlockReason", OnReasonSubmitted);


            Task.Run(async () => await _hubConnection.StartAsync());
        }

        private void OnReasonSubmitted(UnlockReasonData reason)
        {

        }

        private async Task StartTracking()
        {
            await _hubConnection.InvokeAsync("SendCommand", "START");
        }

        private async Task StopTracking()
        {
            await _hubConnection.InvokeAsync("SendCommand", "STOP");
        }
        private async void ShowUnlockReasonPrompt()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.Parent.Parent.FullName;
            string exePath = Path.Combine(projectRoot, @"EmployeeTrackerApp\bin\Debug\net8.0-windows\EmployeeTrackerApp.exe");

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
