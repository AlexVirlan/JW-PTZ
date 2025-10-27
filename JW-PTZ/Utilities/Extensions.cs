using JWPTZ.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace JWPTZ.Utilities
{
    public static class Extensions
    {
        #region String
        public static bool INOE(this string? str) => string.IsNullOrEmpty(str);

        public static (bool exists, string path) CombineWithStartupPath(this string fileName)
        {
            if (fileName.INOE()) { return (File.Exists(fileName), fileName); }
            string file = Path.GetFileName(fileName);
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            return (File.Exists(fullPath), fullPath);
        }

        public static T ToEnum<T>(this string str, T defaultValue) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(str)) { return defaultValue; }
            T result;
            return Enum.TryParse(str, true, out result) ? result : defaultValue;
        }

        public static bool Contains(this string str, params string[] @params)
        {
            return @params.Any(param => str.Contains(param, StringComparison.OrdinalIgnoreCase));
        }

        public static string Repeat(this string str, int count = 2)
        {
            if (string.IsNullOrEmpty(str) || count <= 0) { return string.Empty; }
            if (count == 1) { return str; }
            StringBuilder sb = new StringBuilder(str.Length * count);
            for (int i = 0; i < count; i++) { sb.Append(str); }
            return sb.ToString();
        }

        public static T? ToEnumOrNull<T>(this string str, bool ignoreCase = false) where T : struct, Enum
        {
            if (Enum.TryParse<T>(str, ignoreCase, out T result)) { return result; }
            return null;
        }

        public static string ToSentenceCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return str; }
            string result = Regex.Replace(str, "(?<!^)([A-Z])", " $1");
            return char.ToUpper(result[0]) + result.Substring(1).ToLower();
        }
        #endregion

        #region RichTextBox
        public static void AppendFormattedText(this RichTextBox rtb, string text,
            bool bold = false, bool italic = false, bool underline = false, Brush? brush = null, bool appendNewLine = false)
        {
            if (text.INOE()) { return; }

            Run run = new Run(text);
            if (bold) { run.FontWeight = FontWeights.Bold; }
            if (italic) { run.FontStyle = FontStyles.Italic; }
            if (underline) { run.TextDecorations = TextDecorations.Underline; }
            if (brush != null) { run.Foreground = brush; }
            if (rtb.Document == null) { rtb.Document = new FlowDocument(); }

            Paragraph? paragraph = rtb.Document.Blocks.LastBlock as Paragraph;
            if (paragraph == null)
            {
                paragraph = new Paragraph();
                rtb.Document.Blocks.Add(paragraph);
            }

            paragraph.Inlines.Add(run);
            if (appendNewLine) { paragraph.Inlines.Add(new LineBreak()); }
        }
        #endregion

        #region Others
        public static bool IsChecked(this CheckBox? checkBox)
        {
            return checkBox?.IsChecked is null ? false : (bool)checkBox.IsChecked;
        }

        public static void Toggle(this CheckBox checkbox)
        {
            bool? currentValue = checkbox.IsChecked;
            checkbox.IsChecked = !(currentValue == true);
        }

        public static string ToLowerString(this LogType logType) => logType.ToString().ToLower();
        public static string ToLowerString(this ProtocolType protocolType) => protocolType.ToString().ToLower();

        public static void SetEnabled(this Button button, bool enabled)
        {
            button.IsEnabled = enabled;
            button.Opacity = enabled ? 1.0 : 0.5;
        }
        #endregion
    }
}
