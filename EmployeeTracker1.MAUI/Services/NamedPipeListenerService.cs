using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Services
{
    public class NamedPipeListenerService
    {
        private const string PipeName = "EmployeeTrackerUnlockPipe";
        private CancellationTokenSource _cts;
        public event Action OnSystemUnlocked;

        public void StartListening()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ListenForUnlock(_cts.Token));
        }

        public void StopListening()
        {
            _cts?.Cancel();
        }

        private async Task ListenForUnlock(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                    {
                        await pipeServer.WaitForConnectionAsync(token);
                        using (StreamReader reader = new StreamReader(pipeServer))
                        {
                            string message = await reader.ReadLineAsync();
                            if (message == "UNLOCKED")
                            {
                                OnSystemUnlocked?.Invoke();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Pipe connection error: {ex.Message}");
                }
            }
        }
    }
}
