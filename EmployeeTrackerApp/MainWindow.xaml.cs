using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WebSocketSharp;

namespace EmployeeTrackerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _hubConnection;

        public MainWindow()
        {
            InitializeComponent();
            DisableShortcuts();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/trackerHub")
                .Build();

            Task.Run(async () => await _hubConnection.StartAsync());

            this.Topmost = true;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Show();
            BringToFront();

            this.MouseMove += MainWindow_MouseMove;
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            string reason = (ReasonDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Unknown";
            string details = AdditionalDetails.Text.Trim();

            var reasonData = new
            {
                Reason = reason,
                Details = details
            };

            try
            {
                await _hubConnection.InvokeAsync("SendUnlockReason", reasonData);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending reason: {ex.Message}");
            }
        }

        private void DisableShortcuts()
        {
            this.Topmost = true;
            this.Activate();
            this.Focus();
            this.Closing += (s, e) => e.Cancel = true;
            this.Deactivated += (s, e) => this.Activate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Activate();
            this.Focus();
            BringToFront();
        }

        private void BringToFront()
        {
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetForegroundWindow(handle);
        }

        private void ForceShowWindow()
        {
            this.Show();
            this.Activate();
            this.Focus();
            BringToFront();
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.IsActive || !this.Topmost)
            {
                this.Topmost = true;
                this.Activate();
                this.Focus();
                BringToFront();
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}