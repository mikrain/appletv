using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppleTvSSL;
using Common;

namespace AppleTvService
{
    public partial class AppletvLiar : ServiceBase
    {



        private static ILiar calcInstance;

        public AppletvLiar()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        private string _xmlPath;
        private static string _updateUrl;

        public void Start()
        {
            _xmlPath = ConfigurationSettings.AppSettings["xmlPath"];
            _updateUrl = ConfigurationSettings.AppSettings["updateUrl"];
            if (!string.IsNullOrEmpty(_updateUrl))
            {
                var interval = double.Parse(ConfigurationSettings.AppSettings["updateInterval"]);
                var t = new System.Timers.Timer();
                t.Interval = interval;
                t.Elapsed += t_Elapsed;
                t.Start();
                new UpdateManager().CleanUpdates(_xmlPath);
            }

            ThreadPool.QueueUserWorkItem(obj => new StartTcp().Start(_xmlPath));
            Assembly testAssembly = Assembly.LoadFile(Path.Combine(_xmlPath, "AppleTvLiar.dll"));
            Type calcType = testAssembly.GetType("AppleTvLiar.DNS.DNSService");
            calcInstance = (ILiar)Activator.CreateInstance(calcType);
            calcInstance.Init(_xmlPath);
            StartService(_xmlPath);
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Callback(_xmlPath);
        }

        private static void StartService(string xmlPath, string name = "AppleTvLiar.dll")
        {
            try
            {
                Assembly testAssembly = Assembly.LoadFile(Path.Combine(xmlPath, name));
                Type calcType = testAssembly.GetType("MikrainService.MikrainProgramm");
                calcInstance = (ILiar)Activator.CreateInstance(calcType);
                calcType.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                    null, calcInstance, new object[] { xmlPath });
            }
            catch (Exception exception)
            {
                var ac = new EventLog("logName");
                ac.WriteEntry(exception.ToString(), EventLogEntryType.Error);
            }
        }


        private static async void Callback(object state)
        {
            Console.WriteLine("refresh");
            var manager = new UpdateManager();
            var updateConfig = manager.CheckUpadate(state.ToString(), _updateUrl);

            if (updateConfig != null)
            {
                try
                {
                    if (calcInstance != null)
                    {
                        calcInstance.Dispose();
                        calcInstance = null;
                    }
                    var name = await manager.StartUpdate(updateConfig, state.ToString());
                    StartService(state.ToString(), name);
                }
                catch
                {
                    StartService(state.ToString());
                }
                finally
                {

                }
            }
        }

        protected override void OnStop()
        {
        }
    }
}
