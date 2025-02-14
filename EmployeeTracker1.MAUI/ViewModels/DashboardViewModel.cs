using EmployeeTracker1.MAUI.Helpers;
using EmployeeTracker1.MAUI.Services;
using EmployeeTracker1.MAUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class DashboardViewModel: BaseViewModel
    {
        private readonly NamedPipeClientService _pipeClientService;
        private readonly NamedPipeListenerService _pipeListenerService;
        private readonly UnlockReasonViewModel _unlockReasonViewModel;

        public ICommand StartTrackingCommand { get; }
        public ICommand StopTrackingCommand { get; }

        public string WelcomeMessage { get; private set; } = "Welcome, User!";

        public DashboardViewModel()
        {
            _pipeClientService = new NamedPipeClientService();
            _pipeListenerService = new NamedPipeListenerService();
            _unlockReasonViewModel = new UnlockReasonViewModel();

            StartTrackingCommand = new Command(async () => await StartTracking());
            StopTrackingCommand = new Command(async () => await StopTracking());
        }

        private async Task StartTracking()
        {
            await _pipeClientService.SendMessageAsync("START");
            _pipeListenerService.StartListening();
        }

        private async Task StopTracking()
        {
            await _pipeClientService.SendMessageAsync("STOP");
            _pipeListenerService.StopListening();
        }
        private async void ShowUnlockReasonPrompt()
        {

        }
    }
}
