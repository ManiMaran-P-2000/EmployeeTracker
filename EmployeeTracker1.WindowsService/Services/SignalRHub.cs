using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.WindowsService.Services
{
    public class SignalRHub : Hub
    {
        private readonly IdleTrackerService _trackerService;

        public SignalRHub(IdleTrackerService trackerService)
        {
            _trackerService = trackerService;
        }

        public void SendCommand(string command)
        {
            if (command == "START")
            {
                _trackerService.EnableTracking();
            }
            else if (command == "STOP")
            {
                _trackerService.DisableTracking();
            }
        }

        public async Task SendUnlockReason(UnlockReasonData data)
        {
            await Clients.All.SendAsync("ReceiveUnlockReason", data);
        }
    }

    public class UnlockReasonData
    {
        public string Reason { get; set; }
        public string Details { get; set; }
    }
}
