using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestApp.Properties;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                try
                {
                    string name = Dns.GetHostEntry(args[0]).HostName;
                    //string name = Dns.GetHostEntry(args[0]).HostName);
                    Console.WriteLine(name);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //SearchResultCollection searchResults = null;
            //try
            //{
            //    DirectoryEntry directoryEntry = new DirectoryEntry(Settings.Default.LdapPath);

            //    DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
            //    directorySearcher.Filter = "(&(objectClass=computer)(objectCategory=computer))";

            //    searchResults = directorySearcher.FindAll();

            //    String accountName = "";
            //    String extension = "";

            //    foreach (SearchResult searchResult in searchResults)
            //    {
            //        var computerEntry = searchResult.GetDirectoryEntry();
            //        PropertyCollection props = computerEntry.Properties;

            //        foreach (String property in props.PropertyNames)
            //        {
            //            Console.WriteLine("Property: " + property + ", value: " + computerEntry.Properties[property].Value);
            //        }
            //        Console.WriteLine();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //finally
            //{
            //    if (searchResults != null)
            //        searchResults.Dispose();
            //}
         
        }
    }
}
