using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Services
{
    public class NamedPipeClientService
    {
        private const string PipeName = "EmployeeTrackerCommandPipe";

        public async Task SendMessageAsync(string message)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    int retryCount = 5;
                    while (!pipeClient.IsConnected && retryCount > 0)
                    {
                        try
                        {
                            await pipeClient.ConnectAsync(2000); // Try connecting
                        }
                        catch (TimeoutException)
                        {
                            retryCount--;
                            if (retryCount == 0)
                            {
                                throw new Exception("Failed to connect to Named Pipe Server.");
                            }
                            await Task.Delay(1000);
                        }
                    }

                    using (var writer = new StreamWriter(pipeClient))
                    {
                        writer.AutoFlush = true;
                        await writer.WriteLineAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to service: {ex.Message}");
            }
        }
    }
}
