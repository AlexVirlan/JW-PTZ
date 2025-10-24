using JWPTZ.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace JWPTZ.Utilities
{
    public class SettingsNotifier : Freezable, INotifyPropertyChanged
    {
        protected override Freezable CreateInstanceCore() => new SettingsNotifier();

        public SettingsNotifier()
        {
            Settings.CamerasChanged += (s, e) => OnPropertyChanged(nameof(Cameras));
        }

        public ObservableCollection<PTZCamera> Cameras => Settings.Cameras;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Settings
    {
        #region Properties
        #region Cameras
        private static ObservableCollection<PTZCamera> _cameras = new ObservableCollection<PTZCamera>();
        public static ObservableCollection<PTZCamera> Cameras
        {
            get => _cameras;
            set { if (_cameras != value) { _cameras = value; OnCamerasChanged(); } }
        }

        public static event EventHandler? CamerasChanged;
        private static void OnCamerasChanged()
        {
            CamerasChanged?.Invoke(null, EventArgs.Empty);
        }
        #endregion

        public static AppSettings App { get; set; } = new();
        public static PTZFSpeeds PTZF { get; set; } = new();
        public static ImageSettings Image { get; set; } = new();
        public static UILogsSettings UILogs { get; set; } = new();
        #endregion

        #region Methods
        public static void ReindexCameras()
        {
            for (int i = 0; i < Cameras.Count; i++)
            {
                Cameras[i].Id = i + 1;
            }
        }

        public static List<PTZCamera>? GetCamerasWithDuplicateIds()
        {
            HashSet<int> duplicateIds = Cameras
                .GroupBy(cam => cam.Id)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key)
                .ToHashSet();

            List<PTZCamera> camerasWDI = Cameras
                .Where(cam => duplicateIds.Contains(cam.Id))
                .ToList();

            return camerasWDI.Count == 0 ? null : camerasWDI;
        }
        #endregion
    }

    public class AppSettings
    {
        public int CommandTimeout { get; set; } = 3000;
        public bool SnapshotOnSetPreset { get; set; } = true;
        public bool ButtonsWaitForResponse { get; set; } = false;
        public bool AutoSaveCamsSettings { get; set; } = false;
        public double Opacity { get; set; } = 1;
    }

    public class UILogsSettings
    {
        public bool Visible { get; set; } = true;
        public bool AutoScroll { get; set; } = true;
        public bool ShowTimestamp { get; set; } = true;
        public bool IncludeParams { get; set; } = true;
        public bool VerboseErrors { get; set; } = false;
        public bool ShowFullEndpoint { get; set; } = false;
    }

    [Serializable]
    public class SettingsManager : Settings
    {
        #region Properties
        [JsonProperty("Cameras")]
        public ObservableCollection<PTZCamera> cameras { get { return Cameras; } set { Cameras = value; } }

        [JsonProperty("AppSettings")]
        public AppSettings appSettings { get { return App; } set { App = value; } }

        [JsonProperty("PTZFSpeeds")]
        public PTZFSpeeds ptzfSpeeds { get { return PTZF; } set { PTZF = value; } }

        [JsonProperty("ImageSettings")]
        public ImageSettings imageSettings { get { return Image; } set { Image = value; } }

        [JsonProperty("UILogsSettings")]
        public UILogsSettings uiLogsSettings { get { return UILogs; } set { UILogs = value; } }
        #endregion

        #region Methods
        public static MethodResponse Save(string fileName = "App.set", bool inAppData = false)
        {
            try
            {
                (bool exists, string path) = GetSetFilePath(fileName, inAppData);

                if (!exists)
                {
                    string? folders = Path.GetDirectoryName(path);
                    if (folders.INOE())
                    { return MethodResponse.UnsuccessfulWithMessage("Can't get the path for the settings file."); }
                }

                string settingsData = JsonConvert.SerializeObject(new SettingsManager(), Formatting.Indented);
                File.WriteAllText(path, settingsData);
                return MethodResponse.SuccessfulWithMessage("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                return new MethodResponse(ex);
            }
        }

        public static MethodResponse Load(string fileName = "App.set", bool fromAppData = false)
        {
            try
            {
                (bool exists, string path) = GetSetFilePath(fileName, fromAppData);
                if (!exists)
                {
                    return MethodResponse.UnsuccessfulWithMessage($"The settings file ({path}) is missing.");
                }

                string settingsData = File.ReadAllText(path);
                JsonConvert.DeserializeObject<SettingsManager>(settingsData,
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                return MethodResponse.SuccessfulWithMessage("Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                return new MethodResponse(ex);
            }
        }

        public static (bool exists, string path) GetSetFilePath(string fileName, bool inOrFromAppData = false)
        {
            if (fileName.Trim().INOE()) { fileName = "App.set"; }
            if (inOrFromAppData)
            {
                string appName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Json2Text";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                    $@"\AvA.Soft\{appName}\{fileName}";
                return (File.Exists(path), path);
            }
            else
            {
                return fileName.CombineWithStartupPath();
            }
        }
        #endregion
    }
}
