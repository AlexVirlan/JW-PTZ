using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWPTZ.Entities
{
    public enum WindowType
    {
        Settings, Sync
    }

    public enum ViewType
    {
        Main, Settings
    }

    public enum ProtocolType
    {
        HTTP, HTTPS
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogType
    {
        Debug, Warning, Error, Command
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandType
    {
        None,
        PanLeft, PanRight,
        TiltUp, TiltDown,
        PanTiltStop,
        PanLeftTiltUp, PanRightTiltUp, PanLeftTiltDown, PanRightTiltDown,
        PanTiltFine, PanTiltReset,
        ZoomIn, ZoomOut, ZoomStop, ZoomFine,
        FocusIn, FocusOut, FocusStop,
        ActivateAutoFocus,
        GoHome, SetPreset, CallPreset,
        AdjustLuminance, AdjustSaturation, AdjustContrast, AdjustSharpness, AdjustHue,
        OsdOn, OsdOff, OsdUp, OsdDown, OsdLeft, OsdRight, OsdEnter, OsdBack
    }

    public enum UILogType
    {
        Info, Command
    }
}
