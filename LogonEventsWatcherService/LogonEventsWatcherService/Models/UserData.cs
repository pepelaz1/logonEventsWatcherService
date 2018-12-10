using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService.Models
{
    [Serializable]
    public class UserData
    {
        public String AccountName { get; set; }

        public String Extension { get; set; }
    }
}
