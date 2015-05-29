using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppleTvLiar.DNS;
using AppleTvService;
using AppleTvSSL;
using MikrainService;


namespace AppleTest
{
    class Program
    {
        static void Main(string[] args)
        {

           var _appletvLiar = new AppletvLiar();
            _appletvLiar.Start();

           // ThreadPool.QueueUserWorkItem(obj => new StartTcp().Start(ConfigurationSettings.AppSettings["xmlPath"]));
           // ThreadPool.QueueUserWorkItem(obj => new MikrainProgramm().Init(ConfigurationSettings.AppSettings["xmlPath"]));
           //ThreadPool.QueueUserWorkItem(obj => new DNSService());
            
            //var liar = new AppletvLiar();
            //liar.Start();
            Console.WriteLine("Started");
            Console.ReadLine();
        }
    }
}
