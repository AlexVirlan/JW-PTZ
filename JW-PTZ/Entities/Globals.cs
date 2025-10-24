using JWPTZ.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JWPTZ.Entities
{
    public static class Globals
    {
        public static readonly string GreenHex = "#FF00FF00";
        public static readonly string RedHex = "#FFFF0000";
        public static readonly string DarkRedHex = "#FFAA0000";
        public static readonly string LightRedHex = "#FFF65A5A";
        public static readonly string GrayHex = "#FF404040";
        public static readonly string Gray17Hex = "#FF111111";
        public static readonly string Gray26Hex = "#FF1A1A1A";
        public static readonly string Gray200Hex = "#FFC8C8C8";

        public static readonly SolidColorBrush RedText = Helpers.GetBrush(255, 170, 170);
        public static readonly SolidColorBrush GreenText = Helpers.GetBrush(170, 255, 170);
    }
}
