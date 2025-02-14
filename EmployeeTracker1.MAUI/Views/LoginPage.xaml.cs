using EmployeeTracker1.MAUI.ViewModels;

namespace EmployeeTracker1.MAUI.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel; 
    }
}