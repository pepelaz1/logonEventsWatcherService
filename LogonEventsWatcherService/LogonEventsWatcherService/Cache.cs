using LogonEventsWatcherService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    public static class Cache
    {
        public static Dictionary<String, ADData> Dict { get; } = new Dictionary<string, ADData>();
    }
}
