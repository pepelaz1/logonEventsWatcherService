using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    public static class Utils
    {
        public static String FormatStackTrace(StackTrace st)
        {
            String result = st.GetFrame(0).GetMethod().ReflectedType.ToString() + "."
                + st.GetFrame(0).GetMethod().Name;
            return result;
        }
    }
}
