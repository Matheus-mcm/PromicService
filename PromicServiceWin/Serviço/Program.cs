using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PromicService
{
    internal static class Program
    {
        static void Main()
        {
#if (DEBUG)
            PromicService service = new PromicService();
            service.Start();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PromicService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
