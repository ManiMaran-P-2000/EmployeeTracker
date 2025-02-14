using EmployeeTracker1.MAUI.ViewModels;

namespace EmployeeTracker1.MAUI.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel dashboardViewModel)
	{
		InitializeComponent();
		BindingContext = dashboardViewModel;
    }
}