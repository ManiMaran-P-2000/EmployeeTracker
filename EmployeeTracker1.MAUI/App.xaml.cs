using EmployeeTracker1.MAUI.Views;

namespace EmployeeTracker1.MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
