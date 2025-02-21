using EmployeeTracker1.MAUI.ViewModels;

namespace EmployeeTracker1.MAUI.Views;

public partial class UnlockReasonPage : ContentPage
{
	public UnlockReasonPage(UnlockReasonViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}