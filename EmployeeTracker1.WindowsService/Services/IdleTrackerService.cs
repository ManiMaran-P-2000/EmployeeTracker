using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EmployeeTracker1.WindowsService.Services
{
    public class IdleTrackerService : IHostedService, IDisposable
    {
        private readonly ILogger<IdleTrackerService> _logger;
        private System.Timers.Timer _timer;
        private const int IdleThresholdMinutes = 30;
        private bool _wasLocked = false;
        private bool _isTrackingEnabled = false;

        public IdleTrackerService(ILogger<IdleTrackerService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += CheckIdleTime;
            _timer.Start();

            StartNamedPipeServer(); // Start listening for messages

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
            _isTrackingEnabled = true;
            _logger.LogInformation("Idle tracking enabled.");
        }

        public void DisableTracking()
        {
            _isTrackingEnabled = false;
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

        private async void StartNamedPipeServer()
        {
            _logger.LogInformation("Starting Named Pipe Server...");

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        using (var pipeServer = new NamedPipeServerStream("EmployeeTrackerCommandPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                        {
                            await pipeServer.WaitForConnectionAsync();
                            using (var reader = new StreamReader(pipeServer))
                            {
                                string message = await reader.ReadLineAsync();
                                if (!string.IsNullOrEmpty(message))
                                {
                                    _logger.LogInformation($"Received message: {message}");

                                    if (message == "START")
                                        EnableTracking();
                                    else if (message == "STOP")
                                        DisableTracking();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in Named Pipe Server: {ex.Message}");
                    }
                }
            });
        }

        private void NotifyUnlockEvent()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.FullName;
                string exePath = Path.Combine(projectRoot, @"EmployeeTrackerApp\bin\Debug\net8.0-windows\EmployeeTrackerApp.exe");


                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    process.WaitForInputIdle();

                    IntPtr hWnd = IntPtr.Zero;
                    int retries = 0;

                    while (hWnd == IntPtr.Zero && retries < 10)
                    {
                        Thread.Sleep(500); 
                        hWnd = process.MainWindowHandle;
                        retries++;
                    }

                    if (hWnd != IntPtr.Zero)
                    {
                        SetForegroundWindow(hWnd);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error launching WPF overlay: {ex.Message}");
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void LockWorkStation();
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

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
