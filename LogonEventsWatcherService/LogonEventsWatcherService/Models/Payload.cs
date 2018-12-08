using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService.Models
{
    [JsonObject]
    class Payload
    {
        [JsonProperty]
        public String mac;

        [JsonProperty]
        public String extension;

        [JsonProperty]
        public String pc;

        [JsonProperty]
        public String domain;

        [JsonProperty]
        public String username;
    }
}
