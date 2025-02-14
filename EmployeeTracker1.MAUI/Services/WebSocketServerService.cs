using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Services
{
    public class WebSocketServerService
    {
        private HttpListener _httpListener;
        private List<WebSocket> _clients = new List<WebSocket>();
        private readonly ILogger<WebSocketServerService> _logger;

        public event Action<string> OnReasonReceived;

        public WebSocketServerService(ILogger<WebSocketServerService> logger)
        {
            _logger = logger;
        }

        public async Task StartServer()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:5001/");
            _httpListener.Start();
            _logger.LogInformation("WebSocket Server started on ws://localhost:5001");

            while (true)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;
                    _clients.Add(webSocket);
                    _ = ReceiveMessages(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task ReceiveMessages(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                OnReasonReceived?.Invoke(receivedMessage); // Notify MAUI app with received reason
            }
        }

        public async Task StopServer()
        {
            foreach (var client in _clients)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
            }
            _httpListener.Stop();
        }
    }
}
