using EmployeeTracker1.MAUI.Helpers;
using EmployeeTracker1.MAUI.Models;
using EmployeeTracker1.MAUI.Services;
using System.Windows.Input;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly SignalRClientService _signalRService;
        private readonly BackgroundProcessService _backgroundService;
        private readonly UnlockReasonViewModel _unlockReasonViewModel;
        private TimeSpan _elapsedTime;
        private bool _isTimerRunning;
        private CancellationTokenSource _cts;
        private DateTime _startTime;

        public ICommand StartTrackingCommand { get; }
        public ICommand StopTrackingCommand { get; }

        public string WelcomeMessage { get; private set; } = "Welcome, User!";

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set => SetProperty(ref _elapsedTime, value);
        }

        public DashboardViewModel(SignalRClientService signalRService, BackgroundProcessService backgroundService
            , UnlockReasonViewModel unlockReasonViewModel)
        {
            _signalRService = signalRService;
            _backgroundService = backgroundService;
            _unlockReasonViewModel = unlockReasonViewModel;

            StartTrackingCommand = new Command(async () => await StartTracking());
            StopTrackingCommand = new Command(async () => await StopTracking());

            _signalRService.OnSystemUnlocked += () =>
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.GoToAsync("///UnlockReasonPage");
                });

            _signalRService.OnSystemLocked += () =>
                MainThread.BeginInvokeOnMainThread(PauseTimer);

            _unlockReasonViewModel.ReasonSubmitted += () => ResumeTimer();

            _elapsedTime = TimeSpan.Zero;
            _isTimerRunning = false;
        }

        private async Task StartTracking()
        {
            try
            {
                _backgroundService.StartBackgroundProcess();
                await Task.Delay(1000); // Wait for process to start
                if (_backgroundService.IsProcessRunning())
                {
                    await _signalRService.StartAsync(); // Start SignalR connection after process is running
                    await _signalRService.SendCommand("START");
                    StartTimer();
                    await Application.Current.MainPage.DisplayAlert("Success", "Tracking started.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Background process failed to start.", "OK");
                }
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
                await _signalRService.StopAsync();
                _backgroundService.StopBackgroundProcess();
                StopTimer();
                if (!_backgroundService.IsProcessRunning())
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Tracking stopped.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to stop background process.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to stop tracking: {ex.Message}", "OK");
            }
        }

        private void StartTimer()
        {
            if (_isTimerRunning) return;

            _isTimerRunning = true;
            _startTime = DateTime.Now - _elapsedTime; // Resume from previous elapsed time
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (_isTimerRunning && !_cts.Token.IsCancellationRequested)
                {
                    ElapsedTime = DateTime.Now - _startTime;
                    await Task.Delay(1000); // Update every second
                }
            }, _cts.Token);
        }

        private void PauseTimer()
        {
            if (!_isTimerRunning) return;
            _isTimerRunning = false;
            _cts?.Cancel();
        }

        private void ResumeTimer()
        {
            if (_isTimerRunning) return;
            StartTimer(); // Resumes from the paused elapsed time
        }

        private void StopTimer()
        {
            if (!_isTimerRunning) return;
            _isTimerRunning = false;
            _cts?.Cancel();
            ElapsedTime = TimeSpan.Zero; // Reset to zero
        }
    }
}