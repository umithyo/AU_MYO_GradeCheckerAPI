using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GradeCheckerAPI.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string OneSignalAPIKey { get; set; }
        public string OneSignalAppID { get; set; }
    }
}
