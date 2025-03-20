using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JWptz.Entities;
using JWptz.Services;
using JWptz.Utilities;

namespace JWptz.Windows
{
    public partial class WinMain : Window
    {
        #region Variables
        private PTZCamera _camera = new();
        #endregion

        public WinMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetView(ViewType.Main);
            AddUILog(UILogType.Info, "App started. Welcome! :)");
        }

        public void SetView(ViewType viewType)
        {
            grdMain.Visibility = grdSettings.Visibility = Visibility.Hidden;
            switch (viewType)
            {
                case ViewType.Main: grdMain.Visibility = Visibility.Visible; break;
                case ViewType.Settings: grdSettings.Visibility = Visibility.Visible; break;
            }
        }

        private void btnBackToMain_Click(object sender, RoutedEventArgs e)
        {
            SetView(ViewType.Main);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SetView(ViewType.Settings);
        }

        private async void TEST_Click(object sender, RoutedEventArgs e)
        {

            PTZCamera cam = new PTZCamera("192.168.0.88", ProtocolType.HTTP);
            cam.UseAuth = true;
            cam.Username = "admin";
            cam.Password = "admin";
            cam.Name = "Cam 1";
            PTZCommand cmd = PTZCommand.CallPresetInit(cam, 1);
            APIBaseResponse response = await APIs.SendCommand(cmd);
            AddUILog(UILogType.Command, null, cmd, response);

            APIImageResponse response2 = await APIs.GetSnapshot(cam);
            imgTEST.Source = response2.BitmapImage;
        }

        private void AddUILog(UILogType logType, string? text = null, PTZCommand? command = null, APIBaseResponse? response = null,
            UILogsSettings? uiLS = null)
        {
            if (logType == UILogType.Info && text.INOE()) { return; }
            if (logType == UILogType.Command && (command is null || response is null)) { return; }
            if (uiLS is null) { uiLS = Settings.UILogsSettings; }

            if (uiLS.ShowTimestamp)
            {
                string timeStamp = $"{DateTime.Now:HH:mm:ss.f}";
                rtbLogs.AppendFormattedText($"[{timeStamp}] ", brush: Brushes.Gray);
            }

            switch (logType)
            {
                case UILogType.Info:
                    rtbLogs.AppendFormattedText($"{text}", appendNewLine: true);
                    break;

                case UILogType.Command:
                    if (response.Successful) { rtbLogs.AppendFormattedText($"> ", brush: Helpers.GetBrushFromHex("#FF00FF00")); }
                    else { rtbLogs.AppendFormattedText($"> ", brush: Helpers.GetBrushFromHex("#FFFF0000")); }

                    rtbLogs.AppendFormattedText($"Camera: ", brush: Helpers.GetBrushFromHex("#FFC8C8C8"));
                    rtbLogs.AppendFormattedText($"{command.Camera.Name} ", bold: true);
                    string camPath = uiLS.ShowFullEndpoint ? response.Endpoint : command.Camera.IP;
                    rtbLogs.AppendFormattedText($"({camPath}). Command: ", brush: Helpers.GetBrushFromHex("#FFC8C8C8"));
                    rtbLogs.AppendFormattedText($"{command.CommandType}", bold: true);

                    if (uiLS.IncludeParams)
                    {
                        string qpStr = APIs.GetCommandParams(command);
                        rtbLogs.AppendFormattedText($"{(qpStr.INOE() ? "" : $" ({qpStr})")}", brush: Helpers.GetBrushFromHex("#FFC8C8C8"));
                    }

                    rtbLogs.AppendFormattedText($". Result: ", brush: Helpers.GetBrushFromHex("#FFC8C8C8"));
                    if (response.StatusCode is not null)
                    { rtbLogs.AppendFormattedText($"{response.StatusCode} ", bold: true); }

                    if (response.Successful) { rtbLogs.AppendFormattedText($"✔ Success!", brush: Helpers.GetBrushFromHex("#FF00FF00"), appendNewLine: true); }
                    else
                    {
                        rtbLogs.AppendFormattedText($"✖ Fail!", brush: Helpers.GetBrushFromHex("#FFFF0000"), appendNewLine: true);
                        if (uiLS.VerboseErrors && !response.Message.INOE())
                        { rtbLogs.AppendFormattedText(response.Message, brush: Helpers.GetBrushFromHex("#FFF65A5A"), appendNewLine: true); }
                    }
                    break;
            }
        }

        private void rtbLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Settings.UILogsSettings.AutoScroll) { rtbLogs.ScrollToEnd(); }
        }

        private void UpdateUILogsSettings(object sender, RoutedEventArgs e)
        {
            Settings.UILogsSettings.ShowTimestamp = chkShowTimeStamp.IsChecked();
            Settings.UILogsSettings.AutoScroll = chkAutoScrollUiLogs.IsChecked();
            Settings.UILogsSettings.IncludeParams = chkIncludeParamsToUiLogs.IsChecked();
            Settings.UILogsSettings.ShowFullEndpoint = chkShowFullEndpointToUiLogs.IsChecked();
            Settings.UILogsSettings.VerboseErrors = chkVerboseErrUiLogs.IsChecked();
        }

        private void lblClearUiLogs_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rtbLogs.Document.Blocks.Clear();
        }
    }
}
