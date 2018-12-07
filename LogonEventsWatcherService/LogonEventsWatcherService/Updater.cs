using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LogonEventsWatcherService
{
    class Updater
    {
        private Timer timer;

        public Updater()
        {
            timer = new Timer(3000D);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(timer_elasped);
        }

        public void Start()
        {
            Logger.Log.Info("Updater started");
        }

        public void Stop()
        {
            Logger.Log.Info("Updater stopped");
        }

        private void timer_elasped(object sender, ElapsedEventArgs e)
        {
            try
            {
               // writeLog(n.ToString());
                //n++;
            }
            catch (Exception ex)
            {
                //writeLog(ex.Message);
            }
        }
    }
}
