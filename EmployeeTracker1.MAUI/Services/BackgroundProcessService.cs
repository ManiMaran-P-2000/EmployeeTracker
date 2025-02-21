using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Services
{
    public class BackgroundProcessService
    {
        private readonly ILogger<BackgroundProcessService> _logger;
        private Process _backgroundProcess;

        public BackgroundProcessService(ILogger<BackgroundProcessService> logger)
        {
            _logger = logger;
        }

        public void StartBackgroundProcess()
        {
            try
            {
                if (_backgroundProcess != null && !_backgroundProcess.HasExited)
                {
                    _logger.LogInformation("Background process already running.");
                    return;
                }

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.Parent.Parent.FullName;
                string backgroundExePath = Path.Combine(projectRoot, "EmployeeTracker1.WindowsService\\bin\\Debug\\net8.0\\EmployeeTracker1.WindowsService.exe");

                if (!File.Exists(backgroundExePath))
                {
                    _logger.LogError($"Background executable not found at {backgroundExePath}");
                    throw new FileNotFoundException("Background executable not found.", backgroundExePath);
                }

                _backgroundProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = backgroundExePath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                _backgroundProcess.OutputDataReceived += (sender, args) => _logger.LogInformation(args.Data);
                _backgroundProcess.ErrorDataReceived += (sender, args) => _logger.LogError(args.Data);

                _backgroundProcess.Start();
                _backgroundProcess.BeginOutputReadLine();
                _backgroundProcess.BeginErrorReadLine();

                _logger.LogInformation("Background process started.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start background process: {ex.Message}");
                throw;
            }
        }

        public void StopBackgroundProcess()
        {
            try
            {
                if (_backgroundProcess != null && !_backgroundProcess.HasExited)
                {
                    _backgroundProcess.Kill();
                    _backgroundProcess.WaitForExit();
                    _backgroundProcess.Dispose();
                    _backgroundProcess = null;
                    _logger.LogInformation("Background process stopped.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to stop background process: {ex.Message}");
            }
        }
    }
}
