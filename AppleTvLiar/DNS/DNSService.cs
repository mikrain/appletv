using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using ARSoft.Tools.Net.Dns;
using AppleTvLiar.Helper;
using Common;

namespace AppleTvLiar.DNS
{
    public class DNSService : ILiar
    {
        private IPAddress _ipSite;
        public DNSService()
        {


        }

        private const int BUFSIZE = 32; // Size of receive buffer

        void MyServerStart()
        {
            int servPort = 53;

            TcpListener listener = null;

            try
            {
                // Create a TCPListener to accept client connections
                listener = new TcpListener(IPAddress.Any, servPort);
                listener.Start();
            }
            catch (SocketException se)
            {
                //Console.ReadKey();
                Environment.Exit(se.ErrorCode);

            }

            byte[] rcvBuffer = new byte[BUFSIZE]; // Receive buffer
            int bytesRcvd; // Received byte count
            for (; ; )
            { // Run forever, accepting and servicing connections
                // Console.WriteLine(IPAddress.Any);
                TcpClient client = null;
                NetworkStream netStream = null;

                String responseData = String.Empty;
                try
                {
                    client = listener.AcceptTcpClient(); // Get client connection
                    netStream = client.GetStream();
                    // Receive until client closes connection, indicated by 0 return value
                    int totalBytesEchoed = 0;
                    while ((bytesRcvd = netStream.Read(rcvBuffer, 0, rcvBuffer.Length)) > 0)
                    {
                        netStream.Write(rcvBuffer, 0, bytesRcvd);
                        responseData += System.Text.Encoding.ASCII.GetString(rcvBuffer, 0, bytesRcvd);
                        totalBytesEchoed += bytesRcvd;
                    }
                    // Close the stream and socket. We are done with this client!
                    netStream.Close();
                    client.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    netStream.Close();
                }
            }
        }


        private void InitServer()
        {
            var server = new DnsServer(IPAddress.Any, 1, 0, ProcessQuery);
            server.Start();
            Console.WriteLine("InitServer");
        }

        private DnsMessageBase ProcessQuery(DnsMessageBase message, IPAddress clientAddress, ProtocolType protocolType)
        {

            message.IsQuery = false;
            var query = message as DnsMessage;
            // check for valid query
            if ((query != null)
              && (query.Questions.Count == 1)
              && (query.Questions[0].RecordType == RecordType.A))
            {
                var ipSite = Dns.GetHostAddresses(query.Questions[0].Name)[0];
                Console.WriteLine("ProcessQuery " + query.Questions[0].Name);

                if (query.Questions[0].Name.Contains("trailers.apple.com"))
                // if (query.Questions[0].Name.Contains("trailers.apple.com") || query.Questions[0].Name.Contains("espn.go.com"))
                {
                    ipSite = _ipSite;
                    // = new IPEndPoint(_ipSite, int.Parse("6666")).Address;
                }

                query.ReturnCode = ReturnCode.NoError;
                query.AnswerRecords.Add(new ARecord(query.Questions[0].Name, 3600, ipSite));
            }
            else
            {
                message.ReturnCode = ReturnCode.ServerFailure;
            }
            return message;
        }

        public void Init(string xmlPath)
        {
            try
            {
                Logger.Write("Init Dns");
                InitServer();
                _ipSite = Helpers.GetIpAddress();
                Logger.Write("IP = " + _ipSite.ToString());
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
                Console.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
