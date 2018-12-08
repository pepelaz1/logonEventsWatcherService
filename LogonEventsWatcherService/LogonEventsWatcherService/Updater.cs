using LogonEventsWatcherService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
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
            timer.Start();
        }

        public void Stop()
        {
            Logger.Log.Info("Updater stopped");
            timer.Stop();
        }

        private void timer_elasped(object sender, ElapsedEventArgs e)
        {
            QueryLdap();
        }

        private void QueryLdap()
        {
            Logger.Log.Info("Updater perform ldap query");
            SearchResultCollection searchResults = null;
          
            try
            {             
         
                DirectoryEntry directoryEntry = new DirectoryEntry(Constants.LdapPath);

                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectClass=user))";

                searchResults = directorySearcher.FindAll();

                String samAccountName = "";
                foreach (SearchResult searchResult in searchResults)
                {
                    var userEntry = searchResult.GetDirectoryEntry();
                    var accountNameProp = userEntry.Properties["SAMAccountName"];
                    if (accountNameProp != null)
                         Logger.Log.Info("Found user: " + accountNameProp.Value);
                }

                if (!String.IsNullOrEmpty(samAccountName))
                {
                    ADData adData = null;
                    if (Cache.Dict.ContainsKey(samAccountName))
                    {
                        adData = Cache.Dict[samAccountName];
                    }
                    else
                    {
                        adData = new ADData();
                        Cache.Dict[samAccountName] = adData;
                    }

                    adData.SamAccountName = samAccountName;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(Utils.FormatStackTrace(new StackTrace()) + ": " + ex.Message);
            }
            finally
            {
                if (searchResults != null)
                    searchResults.Dispose();
            }
        }
    }
}
