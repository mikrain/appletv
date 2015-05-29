using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AppleTvService
{
    static class Program
    {
        private static AppletvLiar _appletvLiar;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new AppletvLiar()
            //};
            _appletvLiar = new AppletvLiar();
            ServiceBase.Run(_appletvLiar);
        }
    }
}
