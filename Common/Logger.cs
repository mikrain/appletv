using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Logger
    {
        static readonly object Lock = new object();

        public static void Write(string text)
        {
            lock (Lock)
            {
                var path = Path.Combine(ConfigurationSettings.AppSettings["xmlPath"], "log.txt");
                if (!File.Exists(path))
                {
                    using (File.Create(path))
                    {

                    }
                }
                File.AppendAllLines(path, new List<string>() { text + " - " + DateTime.Now.ToString() });
            }
        }
    }
}
