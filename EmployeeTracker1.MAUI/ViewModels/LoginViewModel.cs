using EmployeeTracker1.MAUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmployeeTracker1.MAUI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await Login());
        }

        private async Task Login()
        {
            if (true)
            {
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Login failed", "OK");
            }
        }
    }
}
