using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PQCToMES
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
           
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings;
            ServicesToRun = new ServiceBase[]
            {
                new PQCToMESService()
            };
            ServiceBase.Run(ServicesToRun);
            
        }
    }
}
