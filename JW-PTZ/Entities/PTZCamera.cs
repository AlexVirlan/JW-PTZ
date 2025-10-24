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

    public class PTZCamera : INotifyPropertyChanged
    {
        #region Private properties
        private int _id = 0;
        private string _ip = string.Empty;
        private string _name = string.Empty;
        private bool _useAuth = false;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _lockPresets = false;
        private bool _osdMode = false;
        private ProtocolType _protocolType = ProtocolType.HTTP;
        #endregion

        #region Public properties
        public int Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(nameof(Id)); } }
        }

        public string IP
        {
            get => _ip;
            set { if (_ip != value) { _ip = value; OnPropertyChanged(nameof(IP)); } }
        }

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } }
        }

        public bool UseAuth
        {
            get => _useAuth;
            set { if (_useAuth != value) { _useAuth = value; OnPropertyChanged(nameof(UseAuth)); } }
        }

        public string Username
        {
            get => _username;
            set { if (_username != value) { _username = value; OnPropertyChanged(nameof(Username)); } }
        }

        public string Password
        {
            get => _password;
            set { if (_password != value) { _password = value; OnPropertyChanged(nameof(Password)); } }
        }

        public bool LockPresets
        {
            get => _lockPresets;
            set { if (_lockPresets != value) { _lockPresets = value; OnPropertyChanged(nameof(LockPresets)); } }
        }

        [JsonIgnore]
        public bool OsdMode
        {
            get => _osdMode;
            set { if (_osdMode != value) { _osdMode = value; OnPropertyChanged(nameof(OsdMode)); } }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolType ProtocolType
        {
            get => _protocolType;
            set { if (_protocolType != value) { _protocolType = value; OnPropertyChanged(nameof(ProtocolType)); } }
        }
        #endregion

        #region Constructors
        public PTZCamera() { }

        public PTZCamera(string ip, string name)
        {
            _ip = ip;
            _name = name;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }


    //public class PTZCamera
    //{
    //    #region Properties
    //    public int Id { get; set; } = 0;
    //    public string IP { get; set; } = string.Empty;
    //    public string Name { get; set; } = string.Empty;
    //    public bool UseAuth { get; set; } = false;
    //    public string Username { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //    public bool LockPresets { get; set; } = false;

    //    [JsonIgnore]
    //    public bool OsdMode { get; set; } = false;

    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public ProtocolType ProtocolType { get; set; } = ProtocolType.HTTP;
    //    #endregion

    //    #region Constructors
    //    public PTZCamera() { }

    //    public PTZCamera(string ip, string name)
    //    {
    //        IP = ip;
    //        Name = name;
    //    }
    //    #endregion
    //}
}
