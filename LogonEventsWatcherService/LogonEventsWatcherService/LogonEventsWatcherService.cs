using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LogonEventsWatcherService
{
    public partial class LogonEventsWatcherService : ServiceBase
    {
        private Watcher watcher = new Watcher();
        private Updater updater = new Updater();
        private Sender sender = new Sender();
   
        public LogonEventsWatcherService()
        {
            InitializeComponent();
            Logger.Init();
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                watcher.Start();
                updater.Start();
                sender.Start();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

    
            
        protected override void OnStop()
        {
            try
            {
                watcher.Stop();
                updater.Stop();
                sender.Stop();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }
    }
}
