using LogonEventsWatcherService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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

            timer = new Timer(int.Parse(ConfigurationManager.AppSettings["LdapQueryInterval"]) * 1000);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(timer_elasped);
            timer.Start();

            Logger.Log.Info("Updater started, inverval (sec): " + ConfigurationManager.AppSettings["LdapQueryInterval"]);
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
            Logger.Log.Info("Updater perform ldap query, path: " + ConfigurationManager.AppSettings["LdapPath"]);

            QueryUserData();

            QueryComputerData();           
        }

        private void QueryUserData()
        {
            SearchResultCollection searchResults = null;
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(ConfigurationManager.AppSettings["LdapPath"]);

                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectClass=person)(objectCategory=user))";

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
                        if (!Cache.UserData.ContainsKey(accountName))
                        {
                            UserData userData = new UserData();
                            Cache.UserData.Add(accountName, userData);
                            Logger.Log.Info("Updater. Add new user data for user: " + accountName);
                        }
                
                        Cache.UserData[accountName].AccountName = accountName;
                        Cache.UserData[accountName].Extension = extension;
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
                DirectoryEntry directoryEntry = new DirectoryEntry(ConfigurationManager.AppSettings["LdapPath"]);
                
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectClass=computer)(objectCategory=computer))";

                searchResults = directorySearcher.FindAll();

                String computerName = "";
                String mac = "";
                foreach (SearchResult searchResult in searchResults)
                {
                    var computerEntry = searchResult.GetDirectoryEntry();

                    var computerNameProp = computerEntry.Properties["sAMAccountName"];
                    if (computerNameProp != null)
                        computerName = computerNameProp.Value.ToString().Replace("$","");


                    var macProp = computerEntry.Properties["msNPCallingStationID"];
                    if (macProp != null)
                        mac = macProp.Value == null ? "" : macProp.Value.ToString();


                    //Logger.Log.Info("Updater. Found computer: " + computerName + ", mac: " + mac);

                    if (!String.IsNullOrEmpty(computerName))
                    {
                         if (!Cache.ComputerData.ContainsKey(computerName))
                        {
                            ComputerData computerData = new ComputerData();
                            Cache.ComputerData.Add(computerName, computerData);
                            Logger.Log.Info("Updater. Add new computer data for computer: " + computerName);
                        }
                        Cache.ComputerData[computerName].ComputerName = computerName;
                        Cache.ComputerData[computerName].Mac = mac;
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
