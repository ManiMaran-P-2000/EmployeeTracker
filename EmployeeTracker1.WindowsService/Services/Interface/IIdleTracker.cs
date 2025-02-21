using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.WindowsService.Services.Interface
{
    public interface IIdleTracker
    {
        TimeSpan GetIdleTime();
        void LockSystem();
        void EnableTracking();
        void DisableTracking();
        event Action OnSystemUnlocked;
    }
}
