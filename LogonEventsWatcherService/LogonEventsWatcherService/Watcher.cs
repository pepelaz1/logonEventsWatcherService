using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    class Watcher
    {
        private const String managementConnectionString = @"\\127.0.0.1\root\cimv2";
        private const String managementQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance isa \"Win32_NTLogEvent\" AND (TargetInstance.EventCode = '4624' OR TargetInstance.EventCode = '4634') ";
        private ManagementEventWatcher managementEventWatcher;

        public void Start()
        {
            //ConnectionOptions connectionOptions = new ConnectionOptions();
            //connectionOptions.Username = "pepel@testdomain.com";
           // connectionOptions.Password = "Qwerty123";
            // Connect to the remote machine's WMI repository
           // ManagementScope managementScope = new ManagementScope(@"\\testserver\root\cimv2");
            ManagementScope managementScope = new ManagementScope(managementConnectionString);
            managementScope.Connect();

            managementEventWatcher = new ManagementEventWatcher(managementScope, new EventQuery(managementQuery));
            managementEventWatcher.EventArrived += new EventArrivedEventHandler(eventArrived);
            managementEventWatcher.Start();

            Logger.Log.Info("Watcher started");
        }

        public void Stop()
        {
            if (managementEventWatcher != null)
                managementEventWatcher.Stop();
           
            Logger.Log.Info("Watcher stopped");
            GC.Collect();
        }

        private void eventArrived(object sender, EventArrivedEventArgs e)
        {
            PropertyData pd;
            if ((pd = e.NewEvent.Properties["TargetInstance"]) != null)
            {
                ManagementBaseObject mbo = pd.Value as ManagementBaseObject;
                processManagementObject(mbo);
            }
        }

        private void processManagementObject(ManagementBaseObject mbo)
        {
            //var eventCode = mbo.Properties["EventCode"];
            //if (eventCode != null)
            //{
            //    Logger.Log.Info("Event: " + eventCode.Value.ToString());
            //}
            try
            {
                String logString = "Event";
                var eventCodeProp = mbo.Properties["EventCode"];
                if (eventCodeProp != null)
                   logString += "code: " + eventCodeProp.Value.ToString() + ": ";

                var timeWrittenProp = mbo.Properties["TimeWritten"];
                if (timeWrittenProp.Value != null)
                    logString += ", time: " + ManagementDateTimeConverter.ToDateTime(timeWrittenProp.Value.ToString());


                Logger.Log.Info(logString);
                //foreach (var property in mbo.Properties)
                //{
                //    if (property.Name != "Message")
                //    {
                //        Logger.Log.Info(property.Name + ": " 
                //            + (property.Value != null ? property.Value.ToString() : ""));
                //        //if (mbo.Properties["Message"].Value != null)
                //        //{
                //        //    //writeLog(mbo.Properties["Message"].Value.ToString());
                //        //}
                //    }
                //}
            }
            catch(Exception ex)
            {
                Logger.Log.Error(Utils.FormatStackTrace(new StackTrace()) + ": " + ex.Message); 
            }            

        }
    }
}
