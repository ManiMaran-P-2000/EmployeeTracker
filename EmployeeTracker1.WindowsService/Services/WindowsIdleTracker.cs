using EmployeeTracker1.WindowsService.Services.Interface;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EmployeeTracker1.WindowsService.Services
{
    public class WindowsIdleTracker : IIdleTracker, IDisposable
    {
        private readonly ILogger<WindowsIdleTracker> _logger;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IConfiguration _config;
        private System.Timers.Timer _timer;
        private readonly int _idleThresholdSeconds;
        private bool _wasLocked = false;
        private bool _isTrackingEnabled = false;

        public event Action OnSystemUnlocked;

        public WindowsIdleTracker(ILogger<WindowsIdleTracker> logger, IHubContext<SignalRHub> hubContext, IConfiguration config)
        {
            _logger = logger;
            _hubContext = hubContext;
            _config = config;
            _idleThresholdSeconds = 10;//_config.GetValue<int>("IdleThresholdSeconds", 600);
        }

        public void EnableTracking()
        {
            if (_isTrackingEnabled) return;
            _isTrackingEnabled = true;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += CheckIdleTime;
            _timer.AutoReset = true;
            _timer.Start();
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
            if (!_isTrackingEnabled) return;

            var idleTime = GetIdleTime();
            if (idleTime >= TimeSpan.FromSeconds(_idleThresholdSeconds) && !_wasLocked)
            {
                LockSystem();
                _wasLocked = true;
                _logger.LogInformation("System locked due to inactivity.");
            }
            else if (idleTime < TimeSpan.FromSeconds(5) && _wasLocked)
            {
                _wasLocked = false;
                NotifyUnlockEventAsync().GetAwaiter().GetResult();
                _logger.LogInformation("System unlocked.");
            }
        }

        public TimeSpan GetIdleTime()
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

        public void LockSystem()
        {
            LockWorkStation();
        }

        private async Task NotifyUnlockEventAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("OnSystemUnlocked");
                OnSystemUnlocked?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to notify unlock event: {ex.Message}");
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
