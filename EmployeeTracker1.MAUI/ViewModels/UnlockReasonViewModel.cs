using EmployeeTracker1.MAUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class UnlockReasonViewModel : BaseViewModel
    {
        private string _selectedReason;
        private string _reasonDetails;
        public ObservableCollection<string> ReasonOptions { get; } = new ObservableCollection<string>
        {
            "Break", "Meeting", "Lunch", "Other"
        };

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
        public UnlockReasonViewModel()
        {
            SubmitReasonCommand = new Command(async() => await SubmitReason());
        }

        private async Task SubmitReason()
        {
           
        }
    }
}
