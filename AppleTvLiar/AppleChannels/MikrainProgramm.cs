using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using AppleTvLiar.Helper;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Text;
using System.Net.Security;
using AppleTvLiar.ProccessManager;
using ARSoft.Tools.Net.Socket;
using Common;

namespace MikrainService
{
    public class MikrainProgramm : ILiar
    {
        public static string _xmlPath;
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(30);
        private static ServiceHost listener = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public void Init(string xmlPath)
        {
            try
            {
                var interval = double.Parse(ConfigurationSettings.AppSettings["updateInterval"]);
                var t = new System.Timers.Timer();
                t.Interval = interval;
                t.Elapsed += t_Elapsed;
                t.Start();


                _xmlPath = xmlPath;
                ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateErrorHandler;

                listener = new ServiceHost(typeof(MikrainService.MikrainServiceMethods));
                // listener.UnknownMessageReceived += listener_UnknownMessageReceived;
                FixEndpoints(listener);

                //specifies how long the operation has to complete before timing out
                listener.Open(timeout);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Close();
            }
        }



        private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //if (MikrainServiceMethods.DateChannel.AddHours(5) > DateTime.Now)
            if (DateTime.Now.AddHours(-5) > MikrainServiceMethods.DateChannel)
            {
                ProccessManager.KillAce();
            }
        }

        void listener_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static bool IgnoreCertificateErrorHandler(object sender,
    X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Close()
        {
            if (listener != null)
            {
                listener.Close(timeout);
                listener = null;
            }
        }

        private void FixEndpoints(ServiceHost listener)
        {
            string hostName = Dns.GetHostName();

            ServiceEndpointCollection endpointCollection = listener.Description.Endpoints;


            for (int i = 0; i < endpointCollection.Count; i++)
            {
                endpointCollection[i].Address = FixEndpointAddress(endpointCollection[i], hostName);
            }

        }

        private EndpointAddress FixEndpointAddress(ServiceEndpoint endpoint, string hostName)
        {
            var uri = new StringBuilder(endpoint.Address.Uri.AbsoluteUri);
            uri.Replace("localhost", Helpers.GetIpAddress().ToString());
            return new EndpointAddress(uri.ToString());
        }


        public void Dispose()
        {
            Close();
        }
    }
}
