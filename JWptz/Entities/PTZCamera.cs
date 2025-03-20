using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Entities
{
    public class PTZCamera
    {
        #region Properties
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public ProtocolType ProtocolType { get; set; } = ProtocolType.HTTP;
        #endregion

        #region Constructors
        public PTZCamera() { }

        public PTZCamera(string ip, ProtocolType protocolType)
        {
            IP = ip;
            ProtocolType = protocolType;
        }
        #endregion
    }
}
