﻿using LogonEventsWatcherService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    class Watcher
    {
        private ManagementEventWatcher managementEventWatcher;
        private Dictionary<String, int> previousEvents = new Dictionary<String, int>();
        private Dictionary<String, String> userComputers = new Dictionary<String, String>();

        public void Start()
        {
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
                ProcessManagementObject(mbo);
            }
        }

        private void ProcessManagementObject(ManagementBaseObject mbo)
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

               // var computerProp = mbo.Properties["ComputerName"];
               //if (computerProp != null)
                //    eventData.ComputerName = computerProp.Value.ToString();

                var messageProp = mbo.Properties["Message"];
                if (messageProp != null)
                    parseMessage(messageProp.Value.ToString(), eventData);


                if (!String.IsNullOrEmpty(eventData.AccountName) && eventData.AccountName != Constants.System)
                {
                    String id = eventData.AccountName + "|" + eventData.ComputerName;

                    // Check if we sending even with same code twice
                    bool b = false;
                    if (!previousEvents.ContainsKey(id))
                    {
                        previousEvents.Add(id, eventData.EventCode);
                        b = true;
                    }
                    else
                    {
                        if (previousEvents[id] != eventData.EventCode)
                        {
                            previousEvents[id] = eventData.EventCode;
                            b = true;
                        }
                    }

                    if (b)
                    {
                        if (String.IsNullOrEmpty(eventData.ComputerName) && userComputers.ContainsKey(eventData.AccountName))
                            eventData.ComputerName = userComputers[eventData.AccountName];

                        if (!String.IsNullOrEmpty(eventData.ComputerName))
                        {
                            String logString = String.Format("Watcher. Enqueue event code: {0}, username: {1}, computer: {2}, domain: {3}, time: {4}",
                                 eventData.EventCode, eventData.AccountName, eventData.ComputerName, eventData.DomainName, eventData.TimeGenerated.ToString());
                            Logger.Log.Info(logString);

                            Queue.Enqueue(eventData);
                        }
                    }                   
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
                String sourceNetworkAddress = "";
                String workstationName = "";
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == Constants.NewLogon)
                    {
                        if (parts[i + 2] == Constants.System)
                            return;

                        eventData.AccountName = parts[i + 4];
                        eventData.DomainName = parts[i + 6];
                        Logger.Log.Info("Watcher. Parsed New logon: account:  " + eventData.AccountName + ", domain: " + eventData.DomainName);
                    }
                    else if (parts[i] == Constants.WorkstationName)
                    {
                        Logger.Log.Info("Watcher. Parsed Workstation Name: " + parts[i + 1]);
                        workstationName = parts[i + 1];
                    }
                    else if (parts[i] == Constants.SourceNetworkAddress)
                    {
                        Logger.Log.Info("Watcher. Parsed Source Network Address: " + parts[i + 1]);
                        sourceNetworkAddress = parts[i + 1];
                    }
                }

                if (!String.IsNullOrEmpty(workstationName) && workstationName != "-")
                {
                    eventData.ComputerName = workstationName;
                }
                else if (!String.IsNullOrEmpty(sourceNetworkAddress) && sourceNetworkAddress != "-")
                {
                    String hostname = Dns.GetHostEntry(sourceNetworkAddress).HostName;
                    if (!String.IsNullOrEmpty(hostname))
                    {
                        eventData.ComputerName = hostname.Split('.')[0];
                        String logString = String.Format("Watcher. Name from source network address: {0}, hostname: {1} ", sourceNetworkAddress, eventData.ComputerName);
                        Logger.Log.Info(logString);
                    }
                }

                if (!String.IsNullOrEmpty(eventData.AccountName) && !String.IsNullOrEmpty(eventData.ComputerName))
                {
                    if (!userComputers.ContainsKey(eventData.AccountName))
                        userComputers.Add(eventData.AccountName, eventData.ComputerName);

                    userComputers[eventData.AccountName] = eventData.ComputerName;
                }
            }
            else if (eventData.EventCode == Constants.LogoffEventCode)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == Constants.Subject)
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
