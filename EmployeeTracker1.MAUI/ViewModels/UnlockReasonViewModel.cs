using EmployeeTracker1.MAUI.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Runtime.InteropServices;
using EmployeeTracker1.MAUI.Services;
using EmployeeTracker1.MAUI.Services.Interface;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class UnlockReasonViewModel : BaseViewModel
    {
        private SignalRClientService _signalRService;
        private readonly IWindowRestrictionService _windowRestrictionService;
        private string _selectedReason;
        private string _reasonDetails;
        public ObservableCollection<string> ReasonOptions { get; set; }

        public string SelectedReason
        {
            get => _selectedReason;
            set { _selectedReason = value; OnPropertyChanged(); }
        }

        public string ReasonDetails
        {
            get => _reasonDetails;
            set { _reasonDetails = value; OnPropertyChanged(); }
        }

        public event Action ReasonSubmitted;
        public ICommand SubmitReasonCommand { get; }

        public UnlockReasonViewModel(SignalRClientService signalRService, IWindowRestrictionService windowRestrictionService)
        {
            _signalRService = signalRService;
            _windowRestrictionService = windowRestrictionService;

            ReasonOptions = new ObservableCollection<string>
            {
                "--Select Option--",
                "Lunch",
                "Break",
                "Other"
            };

            SubmitReasonCommand = new Command(async () => await SubmitReason());
        }

        public void OnAppearing()
        {
            ResetValues();
            _windowRestrictionService.RestrictWindow();
        }

        public void OnDisappearing()
        {
            _windowRestrictionService.RestoreWindow();
        }

        private void ResetValues()
        {
            SelectedReason = ReasonOptions.First(); 
            ReasonDetails = string.Empty;
        }

        private async Task SubmitReason()
        {
            if (string.IsNullOrWhiteSpace(SelectedReason) || SelectedReason == "--Select Option--")
            {
                return;
            }

            try
            {
                _windowRestrictionService.RestoreWindow();
                await Shell.Current.GoToAsync("//DashboardPage");
                ReasonSubmitted?.Invoke();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to submit reason: {ex.Message}", "OK");
            }
        }
    }
}