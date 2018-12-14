using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Properties;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SearchResultCollection searchResults = null;
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(Settings.Default.LdapPath);

                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(&(objectClass=user)(objectCategory=user))";

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

                    Console.WriteLine("Found user: " + accountName + ", extension: " + extension);                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (searchResults != null)
                    searchResults.Dispose();
            }
        }
    }
}
