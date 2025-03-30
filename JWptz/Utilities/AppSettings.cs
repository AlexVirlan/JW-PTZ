using JWptz.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace JWptz.Utilities
{
    public class Settings
    {
        public static ObservableCollection<PTZCamera> Cameras { get; set; } = new ObservableCollection<PTZCamera>()
        {
            { new PTZCamera() { Id = 1, Name = "Cam 1", IP = "192.168.0.88" } },
            { new PTZCamera() { Id = 2, Name = "Cam 2", IP = "192.168.0.881" } },
            { new PTZCamera() { Id = 3, Name = "Cam 3", IP = "192.168.0.882" } }
        };

        public static List<PTZCamera> Cameras2 { get; set; } = new List<PTZCamera>()
        {
            { new PTZCamera() { Id = 1, Name = "Cam 1", IP = "192.168.0.88" } },
            { new PTZCamera() { Id = 2, Name = "Cam 2", IP = "192.168.0.881" } },
            { new PTZCamera() { Id = 3, Name = "Cam 3", IP = "192.168.0.882" } }
        };

        public static int CommandTimeout { get; set; } = 5000;
        public static bool SnapshotOnSetPreset { get; set; } = true;
        public static PTZFSpeeds PTZFSpeeds { get; set; } = new();
        public static ImageSettings ImageSettings { get; set; } = new();
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
        #region Properties
        [JsonProperty("Cameras")]
        public ObservableCollection<PTZCamera> cameras { get { return Cameras; } set { Cameras = value; } }

        [JsonProperty("CommandTimeout")]
        public int commandTimeout { get { return CommandTimeout; } set { CommandTimeout = value; } }

        [JsonProperty("SnapshotOnSetPreset")]
        public bool snapshotOnSetPreset { get { return SnapshotOnSetPreset; } set { SnapshotOnSetPreset = value; } }

        [JsonProperty("PTZFSpeeds")]
        public PTZFSpeeds ptzfSpeeds { get { return PTZFSpeeds; } set { PTZFSpeeds = value; } }

        [JsonProperty("ImageSettings")]
        public ImageSettings imageSettings { get { return ImageSettings; } set { ImageSettings = value; } }

        [JsonProperty("UILogsSettings")]
        public UILogsSettings uiLogsSettings { get { return UILogsSettings; } set { UILogsSettings = value; } }
        #endregion

        #region Methods
        public static FunctionResponse Save(string fileName = "App.set", bool inAppData = false)
        {
            try
            {
                (bool exists, string path) = GetSetFilePath(fileName, inAppData);

                if (!exists)
                {
                    string? folders = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(folders);
                }

                string settingsData = JsonConvert.SerializeObject(new AppSettings(), Formatting.Indented);
                File.WriteAllText(path, settingsData);
                return new FunctionResponse(error: false, message: "Settings saved successfully.");
            }
            catch (Exception ex)
            {
                return new FunctionResponse(ex);
            }
        }

        public static FunctionResponse Load(string fileName = "App.set", bool fromAppData = false)
        {
            try
            {
                (bool exists, string path) = GetSetFilePath(fileName, fromAppData);
                if (!exists)
                {
                    return new FunctionResponse(error: true, message: $"The settings file ({path}) is missing.");
                }

                string settingsData = File.ReadAllText(path);
                JsonConvert.DeserializeObject<AppSettings>(settingsData,
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                return new FunctionResponse(error: false, message: "Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                return new FunctionResponse(ex);
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
