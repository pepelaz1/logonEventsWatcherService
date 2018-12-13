using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService.Models
{
    [JsonObject]
    class RequestData
    {
        [JsonProperty]
        public String id;

        [JsonProperty]
        public String type;

        [JsonProperty]
        public Double timestamp;

        [JsonProperty]
        public String publisher;

        [JsonProperty]
        public Payload payload;

    }
}
