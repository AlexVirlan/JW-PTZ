using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using JWptz.Entities;
using Newtonsoft.Json;
using System.Windows;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using System.Security.Policy;
using System.Collections.Specialized;
using System.Web;
using System.Windows.Media.Imaging;

namespace JWptz.Utilities
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

    }
}
