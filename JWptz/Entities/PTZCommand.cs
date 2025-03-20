using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Entities
{
    public class PTZCommand
    {
        #region Properties
        public int Preset { get; set; }
        public PTZCamera Camera { get; set; } = new();
        public PTZFSpeeds PTZFSpeeds { get; set; } = new();
        public ImageSettings ImageSettings { get; set; } = new();
        public CommandType CommandType { get; set; } = CommandType.None;
        #endregion

        #region Constructors
        public PTZCommand() { }

        public static PTZCommand SetPresetInit(PTZCamera camera, int preset)
        {
            return new PTZCommand
            {
                Camera = camera,
                CommandType = CommandType.SetPreset,
                Preset = preset
            };
        }

        public static PTZCommand CallPresetInit(PTZCamera camera, int preset)
        {
            return new PTZCommand
            {
                Camera = camera,
                CommandType = CommandType.CallPreset,
                Preset = preset
            };
        }
        #endregion

        #region Methods
        #endregion
    }
}
