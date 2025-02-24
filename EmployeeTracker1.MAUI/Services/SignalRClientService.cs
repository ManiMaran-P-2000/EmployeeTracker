using EmployeeTracker1.MAUI.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Services
{
    public class SignalRClientService
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<SignalRClientService> _logger;

        public event Action OnSystemUnlocked;
        public event Action OnSystemLocked;
        public event Action<UnlockReasonData> OnReasonReceived;

        public SignalRClientService(ILogger<SignalRClientService> logger)
        {
            _logger = logger;
            var hubUrl = /*config?.GetValue<string>("SignalRHubUrl") ??*/ "http://localhost:5000/trackerHub";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("OnSystemUnlocked", () => OnSystemUnlocked?.Invoke());
            _hubConnection.On("OnSystemLocked", () => OnSystemLocked?.Invoke());
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task StopAsync() => await _hubConnection.StopAsync();
        public async Task SendCommand(string command) => await _hubConnection.InvokeAsync("SendCommand", command);
    }
}
