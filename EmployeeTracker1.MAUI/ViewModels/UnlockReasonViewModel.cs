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
            SelectedReason = ReasonOptions.First();

            SubmitReasonCommand = new Command(async () => await SubmitReason());
            _windowRestrictionService.RestrictWindow();
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
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to submit reason: {ex.Message}", "OK");
            }
        }
    }
}