using EmployeeTracker1.MAUI.ViewModels;

namespace EmployeeTracker1.MAUI.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}