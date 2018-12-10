using LogonEventsWatcherService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    public static class Cache
    {
        public static Dictionary<String, UserData> UserData { get; private set; } = new Dictionary<string, UserData>();

        public static Dictionary<String, ComputerData> ComputerData { get; private set; } = new Dictionary<string, ComputerData>();
        

        public static void Serialize()
        {
            try
            {
                string fileName = Path.Combine(Path.GetTempPath(), Constants.DataFile);

                Logger.Log.Info("Cache serialization, file: " + fileName);

                using (Stream stream = File.Open(Constants.DataFile, FileMode.Create))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(stream, Cache.UserData);
                    formatter.Serialize(stream, Cache.ComputerData);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(Utils.FormatStackTrace(new StackTrace()) + ": " + ex.Message);
            }
        }

        public static void Deserialize()
        {
            try
            {                
                string fileName = Path.Combine(Path.GetTempPath(), Constants.DataFile);

                Logger.Log.Info("Cache deserialization, file: " + fileName);

                if (File.Exists(fileName))
                {
                    using (Stream stream = File.Open(Constants.DataFile, FileMode.Open))
                    {
                        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        UserData = (Dictionary<String, UserData>)formatter.Deserialize(stream);
                        ComputerData = (Dictionary<String, ComputerData>)formatter.Deserialize(stream);
                    }
                }
                else
                {
                    Logger.Log.Info("Not exist");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(Utils.FormatStackTrace(new StackTrace()) + ": " + ex.Message);
            }
        }
    }
}
