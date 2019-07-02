using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLAQueue.Models
{
    public enum SLAClass
    {
        BestEffort = 0, //This is a special SLA class that will never be processed unless a processor is idle
        TenMinutes = 600000,
        OneHour = 3600000
    }
    
}
