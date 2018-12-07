using LogonEventsWatcherService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    public static class Queue
    {
        private static ConcurrentQueue<EventData> queue = new ConcurrentQueue<EventData>();
        private static bool cancel = false;

        public static int Count { get;  } = queue.Count;

        public static void Enqueue(EventData eventData)
        {
            queue.Enqueue(eventData);
        }

        public static EventData Dequeue()
        {
            EventData eventData;
            while (!queue.TryDequeue(out eventData))
            {
                if (cancel)
                    break;
                Thread.Sleep(10);
            }

            return eventData;
        }

        public static void Cancel()
        {
            cancel = true;
        }
    }
}
