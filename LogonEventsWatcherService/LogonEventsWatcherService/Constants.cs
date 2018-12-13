using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogonEventsWatcherService
{
    public static class Constants
    {
        public const String ManagementConnectionString = @"\\127.0.0.1\root\cimv2";
        public const String ManagementQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance isa \"Win32_NTLogEvent\" AND (TargetInstance.EventCode = '4624' OR TargetInstance.EventCode = '4634') ";

        public const int LogonEventCode = 4624;
        public const int LogoffEventCode = 4634;

        public const String NewLogonEN = "New Logon:";
        public const String NewLogonRU = "Новый вход:";

        public const String SystemEN = "SYSTEM";
        public const String SystemRU = "СИСТЕМА";

        public const String SubjectEN = "Subject:";
        public const String SubjectRU = "Субъект:";

       
        public const String AdUserLogin = "ad.user.login";
        public const String AdUserLogout = "ad.user.logout";
        public const String Publisher = "ad";

        public const String DataFile = "LogonEventsWatcherService.xml";
    }
}
