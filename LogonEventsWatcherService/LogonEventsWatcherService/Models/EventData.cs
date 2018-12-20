using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService.Models
{
    public class EventData
    {
        public String AccountName { get; set; }

        public String ComputerName { get; set; }

        public String DomainName { get; set; }
        
        public int EventCode { get; set; }

        public DateTime TimeGenerated { get; set; }

        public String LogonID { get; set; }
    }
}
