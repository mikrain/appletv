using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AppleTvSSL
{
    public class StartTcp
    {
        private TcpListener listener;
        private string _xmlPath;
        private int count = 0;
        EventLog myLog = new EventLog();


        public void Start(string xmlPath)
        {

            myLog.Source = "AppleSource";

            // Write an informational entry to the event log.    
            myLog.WriteEntry("Tcp SSL starting");

            try
            {
                _xmlPath = xmlPath;
                Console.WriteLine("Try to start tpc");
                listener = new TcpListener(IPAddress.Any, 443);
                listener.Start();
                listener.BeginAcceptTcpClient(ProcessClient, null);
                Console.WriteLine("Started tpc");
            }
            catch (Exception exception)
            {
                myLog.WriteEntry(exception.ToString(), EventLogEntryType.Error);
                if (count < 3)
                {
                    myLog.WriteEntry("Retry", EventLogEntryType.Information);
                    count++;
                    Start(xmlPath);
                }
            }

        }

        private void ProcessClient(IAsyncResult result)
        {
            Console.WriteLine("Get tcp info");
            var client = listener.EndAcceptTcpClient(result);
            var sslStream = new SslStream(client.GetStream(), false, RemoteTarget, LocalTarget);
            try
            {
                var certificate = new X509Certificate2(Path.Combine(_xmlPath, "cert.p12"), "Aa123456");
                sslStream.AuthenticateAsServer(certificate, false, SslProtocols.Default, true);
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;
                var messageData = ReadMessage(sslStream);
                var text = File.OpenText(Path.Combine(_xmlPath, @"Content\js\application.js")).ReadToEnd();
                byte[] message =
                    Encoding.UTF8.GetBytes(text);
                listener.BeginAcceptTcpClient(ProcessClient, null);
                sslStream.Write(message);

            }
            catch (Exception)
            {
                sslStream.Close();
            }
            finally
            {
                sslStream.Close();
            }
        }

        private object ReadMessage(SslStream sslStream)
        {
            var reader = new StreamReader(sslStream);
            return reader.ReadLine();
        }

        private System.Security.Cryptography.X509Certificates.X509Certificate LocalTarget(object sender, string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection localCertificates, System.Security.Cryptography.X509Certificates.X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return localCertificates[0];
        }

        private bool RemoteTarget(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }


}
