using JWPTZ.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace JWPTZ.Utilities
{
    public class Helpers
    {
        private static string _NL = Environment.NewLine;

        public static void WriteLog(string logInfo = "", LogType logType = LogType.Debug, Exception? exception = null,
            [CallerFilePath] string cfp = "unknown", [CallerMemberName] string cmn = "unknown", [CallerLineNumber] int cln = 0)
        {
            if (exception is null && logInfo.INOE()) { return; }
            if (exception is not null) { logType = LogType.Error; }

            string logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", $"{logType}");
            Directory.CreateDirectory(logFolderPath);
            string logFilePath = Path.Combine(logFolderPath, $"{DateTime.Now:dd.MM.yyyy}-JWptz-{logType.ToLowerString()}-logs.json");

            LogData log = new LogData()
            {
                LogType = logType,
                File = Path.GetFileName(cfp),
                Method = cmn,
                Line = cln,
                Message = exception?.Message,
                StackTrace = exception?.StackTrace,
                LogInfo = logInfo
            };

            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(log.ToJsonString(Formatting.None));
            }
        }

        public static Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            using (var stream = new MemoryStream(byteArray))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public static SolidColorBrush GetBrushFromHex(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }

        public static T ParseEnum<T>(string value, bool strict = false, T? fallback = null) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (fallback.HasValue) { return fallback.Value; }
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
            }

            if (Enum.TryParse<T>(value, true, out T result)) { return result; }

            if (!strict)
            {
                foreach (string name in Enum.GetNames(typeof(T)))
                {
                    if (name.Contains(value, StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)Enum.Parse(typeof(T), name);
                    }
                }
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new ArgumentException($"'{value}' is not a valid value for enum '{typeof(T).Name}'.");
        }

        public static void SavePresetCacheImage(int cameraId, int presetId, BitmapImage? image)
        {
            if (image is null) { return; }
            string imagePath = GetPresetCacheImagePath(cameraId, presetId);

            using (FileStream fileStream = new FileStream(imagePath, FileMode.Create))
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        public static void DeletePresetCacheImage(int cameraId, int presetId)
        {
            string imagePath = GetPresetCacheImagePath(cameraId, presetId);
            if (File.Exists(imagePath)) { File.Delete(imagePath); }
        }

        public static void DeletePresetCacheImage(int cameraId)
        {
            string dataCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Cache");
            if (!Directory.Exists(dataCachePath)) { return; }
            Task task = Task.Run(() => DeleteFiles(dataCachePath, $"Cam{cameraId}-Preset*.jpg"));
        }

        public static void DeleteFiles(string path, string pattern)
        {
            string[] files = Directory.GetFiles(path, pattern);
            foreach (string file in files)
            {
                try { File.Delete(file); }
                catch (Exception) { }
            }
        }

        public static string GetPresetCacheImagePath(int cameraId, int presetId)
        {
            string dataCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Cache");
            Directory.CreateDirectory(dataCachePath);
            return Path.Combine(dataCachePath, $"Cam{cameraId}-Preset{presetId}.jpg");
        }

        public static (bool IsValid, string InvalidParams) CheckIfNullOrEmpty(params (string name, string value)[] @params)
        {
            List<string> invalidParams = [];
            foreach ((string name, string value) in @params)
            {
                if (value.INOE()) { invalidParams.Add($"• {name}"); }
            }
            return (invalidParams.Count == 0, string.Join(_NL, invalidParams));
        }

        public static bool IsValidIPv4(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) { return false; }
            if (IPAddress.TryParse(ipAddress, out IPAddress? address))
            { return address?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork; }
            return false;
        }

        public static SolidColorBrush GetBrush(byte red, byte green, byte blue)
        {
            return new SolidColorBrush(Color.FromRgb(red, green, blue));
        }
    }
}
