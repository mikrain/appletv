using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppleTvLiar.Helper;

namespace AppleTvLiar.ProccessManager
{
    public class ProccessManager
    {
        public void StartChannel(string id)
        {
            try
            {
                KillAce();


                var filePath = ConfigurationSettings.AppSettings["acePlayerPath"];
                var info = new ProcessStartInfo(filePath);
               // info.CreateNoWindow = true;
                info.Arguments = @"-I dummy " + id + " " +
                                 @"--sout=#transcode{vcodec=h264,vb=1372,ab=128,venc=x264}:" +
                                 @"duplicate{dst=std{access=livehttp{seglen=10,splitanywhere=true,delsegs=true,numsegs=5,index=" +
                                Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv\mystream.m3u8") + ",index-url=" +
                                @"http://" + Helpers.GetIpAddress() + "/getstreamts/mystream-########.ts},mux=ts,dst=" +
                                 Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv\mystream-########.ts") + "}}";
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.UseShellExecute = false;

               

                var audioPostProcessing = Process.Start(info);
            }
            catch (Exception ex)
            {

            }

        }

        void audioPostProcessing_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;
        }

        public static void KillAce()
        {
            foreach (Process proc in Process.GetProcessesByName("ace_player"))
            {
                using (proc)
                {
                    proc.Kill();
                    proc.WaitForExit();
                    if (Directory.Exists(Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv")))
                    {
                        foreach (
                            var file in Directory.GetFiles(Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv")))
                        {
                            File.Delete(file);
                        }
                        continue;
                    }
                }
            }

           
        }

        internal void KillProcess()
        {
            foreach (Process proc in Process.GetProcessesByName("ace_player"))
            {
                using (proc)
                {
                    proc.Kill();
                    proc.WaitForExit();
                }
            }

        }
    }
}
