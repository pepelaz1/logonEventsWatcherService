using LogonEventsWatcherService.Models;
using System;
using System.Collections.Concurrent;
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
        private ManagementEventWatcher managementEventWatcher;
   
        public void Start()
        {
            //ConnectionOptions connectionOptions = new ConnectionOptions();
            //connectionOptions.Username = "pepel@testdomain.com";
           // connectionOptions.Password = "Qwerty123";
            ManagementScope managementScope = new ManagementScope(Constants.ManagementConnectionString);
            managementScope.Connect();

            managementEventWatcher = new ManagementEventWatcher(managementScope, new EventQuery(Constants.ManagementQuery));
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
            try
            {
                EventData eventData = new EventData();
                var eventCodeProp = mbo.Properties["EventCode"];
                if (eventCodeProp != null)
                    eventData.EventCode =  int.Parse(eventCodeProp.Value.ToString());
             
                var timeGeneratedProp = mbo.Properties["TimeGenerated"];
                if (timeGeneratedProp.Value != null)
                    eventData.TimeGenerated = ManagementDateTimeConverter.ToDateTime(timeGeneratedProp.Value.ToString());
             
                var messageProp = mbo.Properties["Message"];
                if (messageProp != null)
                    parseMessage(messageProp.Value.ToString(), eventData);

                if (eventData.AccountName != Constants.SystemRU && eventData.AccountName != Constants.SystemEN)
                {
                     String logString = String.Format("Enqueue event code: {0}, username: {1}, domain: {2}, time: {3}",
                        eventData.EventCode, eventData.AccountName, eventData.DomainName, eventData.TimeGenerated.ToString());
                    Logger.Log.Info(logString);
                    Queue.Enqueue(eventData);
                }
            }
            catch(Exception ex)
            {
                Logger.Log.Error(Utils.FormatStackTrace(new StackTrace()) + ": " + ex.Message); 
            }           
       }

        private void parseMessage(String message, EventData eventData)
        {
            String[] parts = message.Split("\r\t\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (eventData.EventCode == Constants.LogonEventCode)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == Constants.NewLogonEN || parts[i] == Constants.NewLogonRU)
                    {
                        eventData.AccountName = parts[i + 4];
                        eventData.DomainName = parts[i + 6];
                        break;
                    }
                }
            }
            else if (eventData.EventCode == Constants.LogoffEventCode)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == Constants.SubjectEN || parts[i] == Constants.SubjectRU)
                    {
                        eventData.AccountName = parts[i + 4];
                        eventData.DomainName = parts[i + 6];
                        break;
                    }
                }
            }
        }
        

    }
}
