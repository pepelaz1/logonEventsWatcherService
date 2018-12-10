using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService.Models
{
    [Serializable]
    public class ComputerData
    {
        public String ComputerName { get; set; }

        public String Mac { get; set; }
    }
}
