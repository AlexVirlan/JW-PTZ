using JWptz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Utilities
{
    public class Settings
    {
        public static List<PTZCamera> Cameras = new List<PTZCamera>()
        {
            { new PTZCamera() { Id = 1, Name = "Cam 1", IP = "192.168.0.88" } },
            { new PTZCamera() { Id = 2, Name = "Cam 2", IP = "192.168.0.88" } },
            { new PTZCamera() { Id = 3, Name = "Cam 3", IP = "192.168.0.88" } }
        };

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
