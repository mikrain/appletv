using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;


namespace AppleTvLiar.Helper
{
    public static class Shuffle
    {

        private static Random rng = new Random();

        public static void ShuffleList<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class Helpers
    {
        public static IPAddress GetIpAddress()
        {
            // Получение имени компьютера.
            String host = System.Net.Dns.GetHostName();
            // Получение ip-адреса.
            var ips = System.Net.Dns.GetHostByName(host).AddressList;
            var ipadress = ConfigurationSettings.AppSettings["ipaddress"];
            if (!string.IsNullOrEmpty(ipadress))
            {
                return IPAddress.Parse(ipadress);
            }

         
            if (ips.Count() > 1)
            {
                var asdc= ips[0].ToString();
            }

            //var ipcorrect = ips.FirstOrDefault(address => address.ToString().StartsWith("192."));

            //return ipcorrect;
            if (ips.Any())
            {
                return ips[0];
            }
            return IPAddress.Parse(ipadress);
        }
    }
}
