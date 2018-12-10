using LogonEventsWatcherService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    class Sender
    {
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private AutoResetEvent resetEvent = new AutoResetEvent(false);

        public Sender()
        {
            backgroundWorker.DoWork += doWork;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        public void Start()
        {
           backgroundWorker.RunWorkerAsync();
           Logger.Log.Info("Sender started");
        }

        public void Stop()
        {
            Queue.Cancel();

            backgroundWorker.CancelAsync();
            resetEvent.WaitOne();
            Logger.Log.Info("Sender stopped");
        }

        private void doWork(object sender, DoWorkEventArgs e)
        {
            Logger.Log.Info("Sender do work");
            while (!e.Cancel)
            {
                EventData eventData = Queue.Dequeue();
                if (eventData != null)
                {
                    String logString = String.Format("Dequeue event code: {0}, username: {1}, computer: domain: {2}, time: {3}",
                        eventData.EventCode, eventData.AccountName, eventData.DomainName, eventData.TimeGenerated.ToString());
                    Logger.Log.Info(logString);
                    Logger.Log.Info("Queue count: " + Queue.Count.ToString());

                    Send(eventData);
                }

                if (backgroundWorker.CancellationPending)
                    break;

            }
            resetEvent.Set();
        }

        private void Send(EventData eventData)
        {
            //{
            //    "id": "8400b42a-0715-4f34-8330-19cc754c804b", // uuid
            //    "type": "ad.user.login", // "type": "ad.user.logout",
            //    "timestamp": 1543939345.173888, // timestamp
            //    "publisher": "ad", // name of service
            //    "payload": {
            //        "mac": ...., // mac address stored in AD computer object
            //        "extension": ...., // extension stored in AD user object
            //        "pc": ...., // pc where event ocurred
            //        "domain": ...., // AD domain
            //        "username": ..... // AD username
            //    }
            //}

            try
            {
                UserData userData = Cache.UserData[eventData.AccountName];
                ComputerData computerData = Cache.ComputerData[eventData.ComputerName];

                Int32 timestamp = (Int32)(eventData.TimeGenerated.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                var requestData = new RequestData()
                {
                    id = Constants.RequestId,
                    type = eventData.EventCode == Constants.LogonEventCode ?
                        Constants.AdUserLogin : Constants.AdUserLogout,
                    timestamp = timestamp,
                    publisher = Constants.Publisher,
                    payload = new Payload()
                    {
                        mac = computerData.Mac,
                        extension = userData.Extension,
                        pc = eventData.ComputerName,
                        domain = eventData.DomainName,
                        username = eventData.AccountName
                    }
                };

                string json = JsonConvert.SerializeObject(requestData);

                var request = WebRequest.Create(Settings.Default.TargetWebServiceUrl);
                request.ContentType = "application/json";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                Logger.Log.Info("Perform http request: ");

                var response = request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Logger.Log.Info("Respose: " + result);
                }
            }
            catch(Exception ex)
            {
                Logger.Log.Error(Utils.FormatStackTrace(new StackTrace()) + ": " + ex.Message);
            }
        }
    }
}
