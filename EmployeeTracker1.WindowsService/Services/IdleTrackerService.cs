using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Timers;

namespace EmployeeTracker1.WindowsService.Services
{
    public class IdleTrackerService : IHostedService, IDisposable
    {
        private readonly ILogger<IdleTrackerService> _logger;
        private IHubContext<SignalRHub> _hubContext;
        private System.Timers.Timer _timer;
        private const int IdleThresholdMinutes = 30;
        private bool _wasLocked = false;
        private bool _isTrackingEnabled = false;

        public IdleTrackerService(ILogger<IdleTrackerService> logger, IHubContext<SignalRHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("IdleTrackerService started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            _logger.LogInformation("IdleTrackerService stopped.");
            return Task.CompletedTask;
        }

        public void EnableTracking()
        {
            if (_isTrackingEnabled)
                return;

            _isTrackingEnabled = true;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += CheckIdleTime;
            _timer.AutoReset = true;
            _timer?.Start();
            _logger.LogInformation("Idle tracking enabled.");

        }

        public void DisableTracking()
        {
            _isTrackingEnabled = false;
            _timer?.Stop();
            _logger.LogInformation("Idle tracking disabled.");
        }

        private void CheckIdleTime(object sender, ElapsedEventArgs e)
        {
            if (!_isTrackingEnabled)
                return;

            var idleTime = GetIdleTime();
            if (idleTime >= TimeSpan.FromSeconds(IdleThresholdMinutes) && !_wasLocked)
            {
                LockWorkStation();
                _wasLocked = true;
                _logger.LogInformation("System locked due to inactivity.");
            }
            else if (idleTime < TimeSpan.FromSeconds(5) && _wasLocked)
            {
                _wasLocked = false;
                NotifyUnlockEvent();
                _logger.LogInformation("System unlocked.");
            }
        }


        private TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
                return TimeSpan.FromMilliseconds(idleTime);
            }
            return TimeSpan.Zero;
        }

        private void NotifyUnlockEvent()
        {
            try
            {
                //_hubContext.Clients.All.SendAsync("OnSystemUnlocked");
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.FullName;
                string exePath = Path.Combine(projectRoot, @"EmployeeTrackerApp\bin\Debug\net8.0-windows\EmployeeTrackerApp.exe");


                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                };

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForInputIdle(); // Wait for the process to be ready for input
                    IntPtr handle = process.MainWindowHandle;
                    SetForegroundWindow(handle); // Bring the window to the front
                    ForceShowWindow(handle); // Ensure window is shown
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error launching WPF overlay: {ex.Message}");
            }
        }

        private void ForceShowWindow(IntPtr handle)
        {
            // Forcibly bring the window to the front and ensure it's activated
            SetForegroundWindow(handle);
            ShowWindow(handle, SW_SHOW);
            ShowWindow(handle, SW_RESTORE);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void LockWorkStation();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
