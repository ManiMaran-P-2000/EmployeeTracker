using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracker1.MAUI.Services.Interface
{
    public interface IWindowRestrictionService
    {
        void RestrictWindow();
        void RestoreWindow();
    }
}
