﻿using System;
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
            // Create the source, if it does not already exist. 
           
            // Create an EventLog instance and assign its source.
            EventLog myLog = new EventLog();
            myLog.Source = "AppleSource";

            // Write an informational entry to the event log.    
            myLog.WriteEntry("Writing to event log.");


            Start();
        }

        private string _xmlPath;
        private static string _updateUrl;

        public void Start()
        {
            if (!EventLog.SourceExists("AppleSource"))
            {
                //An event log source should not be created and immediately used. 
                //There is a latency time to enable the source, it should be created 
                //prior to executing the application that uses the source. 
                //Execute this sample a second time to use the new source.
                EventLog.CreateEventSource("AppleSource", "AppleLog");
                Console.WriteLine("CreatedEventSource");
                Console.WriteLine("Exiting, execute the application a second time to use the source.");
                // The source is created.  Exit the application to allow it to be registered. 
            }

            EventLog myLog = new EventLog();
            myLog.Source = "AppleSource";

            // Write an informational entry to the event log.    
            myLog.WriteEntry("Apple liar started");

            try
            {
                Logger.Write("start service");
           
            _xmlPath = ConfigurationSettings.AppSettings["xmlPath"];
            Logger.Write(_xmlPath);
            _updateUrl = ConfigurationSettings.AppSettings["updateUrl"];
            var interval = double.Parse(ConfigurationSettings.AppSettings["updateInterval"]);
            //var timer = new Timer(Callback, xmlPath, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(40));
            var t = new System.Timers.Timer();
            t.Interval = interval;
            t.Elapsed += t_Elapsed;
            t.Start();

           new UpdateManager().CleanUpdates(_xmlPath);

            ThreadPool.QueueUserWorkItem(obj => new StartTcp().Start(_xmlPath));

            Assembly testAssembly = Assembly.LoadFile(Path.Combine(_xmlPath, "AppleTvLiar.dll"));
            Type calcType = testAssembly.GetType("AppleTvLiar.DNS.DNSService");
            calcInstance = (ILiar)Activator.CreateInstance(calcType);
            calcInstance.Init(_xmlPath);
            //calcType.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
            //    null, calcInstance, new object[] { xmlPath });

            //ThreadPool.QueueUserWorkItem(obj => new DNSService());
            StartService(_xmlPath);

            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString());
                myLog.WriteEntry(ex.ToString(),EventLogEntryType.Error);
            }
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
            catch (Exception)
            {

                throw;
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
                    if (name != null) StartService(state.ToString(), name);
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
