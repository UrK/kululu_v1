using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Dror.Common.Utils.Contracts;

namespace Dror.Common.Utils
{
    public class Diagnostics
    {
        PerformanceCounter cpuCounter;
        
        public Diagnostics()
        {
            cpuCounter = new PerformanceCounter();
            cpuCounter.MachineName = "eweb704";
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
        }

        public string getCurrentCpuUsage()
        {
            return cpuCounter.NextValue() + "%";
        }
    }
}
