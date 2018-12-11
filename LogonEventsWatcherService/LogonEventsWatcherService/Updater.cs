using LogonEventsWatcherService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LogonEventsWatcherService
{
    class Updater
    {
        private Timer timer;
    
 
        public void Start()
        {           
            //Cache.Deserialize();
            QueryLdap();

            timer = new Timer(Settings.Default.LdapQueryInterval * 1000);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(timer_elasped);
            timer.Start();

            Logger.Log.Info("Updater started, inverval (sec): " + Settings.Default.LdapQueryInterval.ToString());
        }

        public void Stop()
        {
            timer.Stop();
            timer = null;

            //Cache.Serialize();

            Logger.Log.Info("Updater stopped");
        }

        private void timer_elasped(object sender, ElapsedEventArgs e)
        {
            QueryLdap();
        }

        private void QueryLdap()
        {
            Logger.Log.Info("Updater perform ldap query");

            QueryUserData();

            QueryComputerData();           
        }

        private void QueryUserData()
        {
            SearchResultCollection searchResults = null;
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(Settings.Default.LdapPath);

                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectClass=user))";

                searchResults = directorySearcher.FindAll();

                String accountName = "";
                String extension = "";

                foreach (SearchResult searchResult in searchResults)
                {
                    var userEntry = searchResult.GetDirectoryEntry();

                    var accountNameProp = userEntry.Properties["sAMAccountName"];
                    if (accountNameProp != null)
                        accountName = accountNameProp.Value.ToString();

                    var extensionProp = userEntry.Properties["ipPhone"];
                    if (extensionProp != null)
                        extension = extensionProp.Value == null ? "" : extensionProp.Value.ToString();

                    //Logger.Log.Info("Updater. Found user: " + accountName + ", extension: " + extension);

                    if (!String.IsNullOrEmpty(accountName))
                    {
                        UserData userData = null;
                        if (Cache.UserData.ContainsKey(accountName))
                        {
                            userData = Cache.UserData[accountName];
                        }
                        else
                        {
                            userData = new UserData();
                            Cache.UserData[accountName] = userData;
                        }

                        userData.AccountName = accountName;
                        userData.Extension = extension;
                    }
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

        private void QueryComputerData()
        {
            SearchResultCollection searchResults = null;
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(Settings.Default.LdapPath);

                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectClass=computer))";

                searchResults = directorySearcher.FindAll();

                String computerName = "";
                String mac = "";
                foreach (SearchResult searchResult in searchResults)
                {
                    var computerEntry = searchResult.GetDirectoryEntry();

                    var computerNameProp = computerEntry.Properties["sAMAccountName"];
                    if (computerNameProp != null)
                        computerName = computerNameProp.Value.ToString();


                    var macProp = computerEntry.Properties["msNPCallingStationID"];
                    if (macProp != null)
                        mac = macProp.Value == null ? "" : macProp.Value.ToString();


                    //Logger.Log.Info("Updater. Found computer: " + computerName + ", mac: " + mac);

                    if (!String.IsNullOrEmpty(computerName))
                    {
                        ComputerData computerData = null;
                        if (Cache.ComputerData.ContainsKey(computerName))
                        {
                            computerData = Cache.ComputerData[computerName];
                        }
                        else
                        {
                            computerData = new ComputerData();
                            Cache.ComputerData[computerName] = computerData;
                        }

                        computerData.ComputerName = computerName;
                        computerData.Mac = mac;
                    }
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
