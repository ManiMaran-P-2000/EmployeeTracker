using EmployeeTracker1.WindowsService.Services.Interface;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeTracker1.WindowsService.Models;

namespace EmployeeTracker1.WindowsService.Services
{
    public class SignalRHub : Hub
    {
        private readonly IIdleTracker _tracker;

        public SignalRHub(IIdleTracker tracker)
        {
            _tracker = tracker;
        }

        public void SendCommand(string command)
        {
            if (command == "START")
            {
                _tracker.EnableTracking();
            }
            else if (command == "STOP")
            {
                _tracker.DisableTracking();
            }
        }
    }
}
