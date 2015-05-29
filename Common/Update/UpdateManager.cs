using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Common
{
    public class UpdateManager
    {
        public UpdateConfig CheckUpadate(string xmlPath, string url)
        {
            try
            {
                // var xmlConfig = XDocument.Load("");
                var serializer = new XmlSerializer(typeof(UpdateConfig));
                var xmlReader = new XmlTextReader(url);
                var remoteConfig = (UpdateConfig)serializer.Deserialize(xmlReader);
                bool shouldBeUpdated = false;
                if (!Directory.Exists(Path.Combine(xmlPath, "Updates"))) Directory.CreateDirectory(Path.Combine(xmlPath, "Updates"));

                using (var stream = new FileStream(Path.Combine(xmlPath, "Updates/UpdateConfig.xml"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    UpdateConfig localConfig = null;
                    if (stream.Length != 0) localConfig = (UpdateConfig)serializer.Deserialize(stream);
                    if (localConfig == null || (remoteConfig.UpdateVersion != localConfig.UpdateVersion))
                    {
                        shouldBeUpdated = true;
                    }
                }

                if (shouldBeUpdated)
                {
                    var writer = new XmlSerializer(typeof(UpdateConfig));
                    var file = new System.IO.StreamWriter(Path.Combine(xmlPath, "Updates/UpdateConfig.xml"));
                    writer.Serialize(file, remoteConfig);
                    file.Close();
                    return remoteConfig;
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task<string> StartUpdate(UpdateConfig updateConfig, string xmlPath)
        {
            try
            {


                var client = new HttpClient();
                string dllName = "AppleTvLiar.dll";
                foreach (var config in updateConfig.Config)
                {
                    if (config.ShouldBeChanged)
                    {
                        try
                        {
                            using (var streamSource = await client.GetStreamAsync(config.Url))
                            {
                                var directorySource = Path.GetDirectoryName(Path.Combine(xmlPath, config.Source));
                                if (directorySource != null && !Directory.Exists(directorySource))
                                    Directory.CreateDirectory(directorySource);


                                string filename = Path.Combine(xmlPath, config.Source) + DateTime.Now.Ticks;
                                using (var fileStream = File.Create(filename))
                                {
                                    streamSource.CopyTo(fileStream);
                                }

                                var directory = Path.GetDirectoryName(Path.Combine(xmlPath, config.Target));
                                if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
                                if (!File.Exists(Path.Combine(xmlPath, config.Target)))
                                {
                                    File.Create(Path.Combine(xmlPath, config.Target)).Dispose();
                                }

                                Directory.CreateDirectory(Path.Combine(xmlPath, "Updates/Removes/"));

                                File.Move(Path.Combine(xmlPath, config.Target),
                                    Path.Combine(xmlPath, "Updates/Removes/") + DateTime.Now.Ticks);

                                var extension = Path.GetExtension(config.Target);
                                if (extension.ToLower() == ".dll")
                                {
                                    dllName = filename;
                                }
                                File.Copy(filename, Path.Combine(xmlPath, config.Target));
                            }
                        }
                        catch
                        {


                        }
                    }
                }
                return dllName;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public void CleanUpdates(string _xmlPath)
        {
            try
            {
                if (Directory.Exists(Path.Combine(_xmlPath, "Updates/Removes")))
                {
                    Directory.Delete(Path.Combine(_xmlPath, "Updates/Removes"), true);
                }
                if (Directory.Exists(Path.Combine(_xmlPath, "Updates/BackupFile")))
                {
                    Directory.Delete(Path.Combine(_xmlPath, "Updates/BackupFile"), true);
                }

            }
            catch (Exception)
            {

            }

        }
    }
}
