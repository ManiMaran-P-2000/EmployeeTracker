using WebSocketSharp;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmployeeTrackerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebSocket _client;

        public MainWindow()
        {
            InitializeComponent();
            DisableShortcuts();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string reason = (ReasonDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Unknown";
            string details = AdditionalDetails.Text.Trim();

            string dataToSend = $"{reason}|{details}";

            Task.Run(() =>
            {
                using (WebSocket client = new WebSocket("ws://localhost:5001/"))
                {
                    client.Connect();
                    client.Send(dataToSend);
                    client.Close();
                }

                Dispatcher.Invoke(() => Application.Current.Shutdown());
            });
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
            this.Activate();
            this.Topmost = true;
            this.Focus();

            Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Topmost = true; 
                this.Focus();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}