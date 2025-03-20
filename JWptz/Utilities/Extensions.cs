using JWptz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace JWptz.Utilities
{
    public static class Extensions
    {
        #region String
        public static bool INOE(this string? str) => string.IsNullOrEmpty(str);
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

        public static string ToLowerString(this LogType logType) => logType.ToString().ToLower();
        public static string ToLowerString(this ProtocolType protocolType) => protocolType.ToString().ToLower();
        #endregion
    }
}
