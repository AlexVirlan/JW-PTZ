using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Utilities
{
    public class Settings
    {
        public static int CommandTimeout { get; set; } = 2500;
        public static UILogsSettings UILogsSettings { get; set; } = new();
    }

    public class UILogsSettings
    {
        public bool ShowTimestamp { get; set; } = true;
        public bool AutoScroll { get; set; } = true;
        public bool IncludeParams { get; set; } = true;
        public bool ShowFullEndpoint { get; set; } = false;
        public bool VerboseErrors { get; set; } = false;
    }

    [Serializable]
    public class AppSettings : Settings
    {

    }
}
