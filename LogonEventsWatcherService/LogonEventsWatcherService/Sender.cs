using LogonEventsWatcherService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                    String logString = String.Format("Dequeue event code: {0}, username: {1}, domain: {2}, time: {3}",
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

        }
    }
}
