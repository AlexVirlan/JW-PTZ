using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Entities
{
    public class PTZFSpeeds
    {
        public int PanSpeed { get; set; } = 8;
        public int TiltSpeed { get; set; } = 8;
        public int ZoomSpeed { get; set; } = 3;
        public int FocusSpeed { get; set; } = 3;
    }

    public class ImageSettings
    {
        public int Luminance { get; set; } = 0;
        public int Saturation { get; set; } = 0;
        public int Contrast { get; set; } = 0;
        public int Sharpness { get; set; } = 0;
        public int Hue { get; set; } = 0;
    }
}
