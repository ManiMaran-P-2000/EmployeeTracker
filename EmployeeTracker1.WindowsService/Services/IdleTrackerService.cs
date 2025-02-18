using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
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
            else if (idleTime < TimeSpan.FromSeconds(2) && _wasLocked)
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

        private async Task NotifyUnlockEvent()
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
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(startInfo);
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
