using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Server.Model;

namespace ListenerService
{
    static class Program
    {
        /// <summary>
        /// Точка входа сервиса
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new ListenerService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
