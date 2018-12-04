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
        private Timer timer;
        private int n = 0;
        private String logFile;
        private ManagementEventWatcher watcher;
        //private EventLogHandler handler;


        public LogonEventsWatcherService()
        {
            InitializeComponent();
            timer = new Timer(3000D);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(timer_elasped);
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                timer.Start();

                ConnectionOptions connectionOptions = new ConnectionOptions();
                connectionOptions.Username = "pepel@testdomain.com";
                connectionOptions.Password = "Qwerty123";
                // Connect to the remote machine's WMI repository
                ManagementScope ms = new ManagementScope(@"\\testserver\root\cimv2");
                // connect it
                ms.Connect();

                watcher = new ManagementEventWatcher(ms, new EventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance isa \"Win32_NTLogEvent\" AND (TargetInstance.EventCode = '4624' OR TargetInstance.EventCode = '4634') "));
                watcher.EventArrived += new EventArrivedEventHandler(eventArrived);
                watcher.Start();

            }
            catch (Exception ex)
            {
                //log anywhere
                writeLog(ex.Message);
            }
        }

        private void eventArrived(object sender, EventArrivedEventArgs e)
        {
            PropertyData pd;
            if ((pd = e.NewEvent.Properties["TargetInstance"]) != null)
            {
                ManagementBaseObject mbo = pd.Value as ManagementBaseObject;
                if (mbo.Properties["Message"].Value != null)
                {
                    writeLog(mbo.Properties["Message"].Value.ToString());
                }
            }
        }


        private void timer_elasped(object sender, ElapsedEventArgs e)
        {
            try
            {
                writeLog(n.ToString());
                n++;
            }
            catch (Exception ex)
            {
                writeLog(ex.Message);
            }
        }
            
        protected override void OnStop()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

       
        private void writeLog(String message)
        {
            if (String.IsNullOrEmpty(logFile))
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                logFile = Path.GetDirectoryName(path) + "\\log.txt";
           }

            File.AppendAllText(logFile, DateTime.Now.ToShortTimeString() + ": " + message +"\n");
        }
    }
}
