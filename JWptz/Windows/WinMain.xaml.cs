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
        private bool _loading = false;
        private PTZCamera? _camera = new();
        #endregion

        public WinMain()
        {
            _loading = true;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetView(ViewType.Main);

            FunctionResponse frLoadSet = AppSettings.Load();
            ApplySettingsToUI();
            UpdateCamInfo();

            AddUILog(UILogType.Info, "App started. Welcome! :)");
            _loading = false;
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

        public void ApplySettingsToUI()
        {
            chkShowTimeStamp.IsChecked = Settings.UILogsSettings.ShowTimestamp;
            chkAutoScrollUiLogs.IsChecked = Settings.UILogsSettings.AutoScroll;
            chkIncludeParamsToUiLogs.IsChecked = Settings.UILogsSettings.IncludeParams;
            chkShowFullEndpointToUiLogs.IsChecked = Settings.UILogsSettings.ShowFullEndpoint;
            chkVerboseErrUiLogs.IsChecked = Settings.UILogsSettings.VerboseErrors;

            sldPanSpeed.Value = Settings.PTZFSpeeds.PanSpeed;
            sldTiltSpeed.Value = Settings.PTZFSpeeds.TiltSpeed;
            sldZoomSpeed.Value = Settings.PTZFSpeeds.ZoomSpeed;
            sldFocusSpeed.Value = Settings.PTZFSpeeds.FocusSpeed;
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
            Settings.Cameras.Clear();
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
                    rtbLogs.AppendFormattedText($"{command.Camera.Id} - {command.Camera.Name} ", bold: true);
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
            if (_loading) { return; }
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

        private async void CallPreset_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                #region Validations
                if (_camera is null)
                {
                    ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                    return;
                }

                Button? presetButton = sender as Button;
                if (presetButton is null) { return; }

                if (!int.TryParse(presetButton.Name.Replace("btnPreset", "", StringComparison.OrdinalIgnoreCase), out int preset))
                { throw new Exception("Could not parse the preset button value."); }
                #endregion

                bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (e.ChangedButton == MouseButton.Left)
                {
                    PTZCommand cmd = PTZCommand.CallPresetInit(_camera, preset);
                    APIBaseResponse response = await APIs.SendCommand(cmd);
                    AddUILog(UILogType.Command, null, cmd, response);
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    PTZCommand cmd = PTZCommand.SetPresetInit(_camera, preset);
                    APIBaseResponse presRes = await APIs.SendCommand(cmd);
                    AddUILog(UILogType.Command, null, cmd, presRes);

                    APIImageResponse imgRes = await APIs.GetSnapshot(_camera);
                    if (imgRes.Successful)
                    {
                        presetButton.Background = new ImageBrush(imgRes.BitmapImage) { Stretch = Stretch.Fill };
                        Helpers.SavePresetCacheImage(_camera.Id, preset, imgRes.BitmapImage);
                    }
                }
                else if (e.ChangedButton == MouseButton.Middle)
                {
                    presetButton.Background = Helpers.GetBrushFromHex("#FF111111");
                    Helpers.DeletePresetCacheImage(_camera.Id, preset);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private async void PTZFControl_PreviewMouseDownUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                #region Validations
                if (_camera is null)
                {
                    ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                    return;
                }

                Button? ptzfButton = sender as Button;
                if (ptzfButton is null) { return; }
                #endregion

                string cmdName = ptzfButton.Name.Replace("btn", "", StringComparison.OrdinalIgnoreCase);
                CommandType cmdType = Helpers.ParseEnum<CommandType>(cmdName);

                if (e.ButtonState == MouseButtonState.Released)
                {
                    cmdType = APIs.GetStopCommandType(cmdType);
                    if (cmdType == CommandType.None) { return; }
                }

                PTZCommand pTZCommand = new PTZCommand()
                {
                    Preset = 0,
                    Camera = _camera,
                    PTZFSpeeds = Settings.PTZFSpeeds,
                    ImageSettings = Settings.ImageSettings,
                    CommandType = cmdType
                };

                APIBaseResponse response = await APIs.SendCommand(pTZCommand);
                AddUILog(UILogType.Command, null, pTZCommand, response);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void cmbCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCamera.SelectedItem is null)
            {
                _camera = null;
                lblCamInfo.Content = "-";
                return;
            }
            _camera = cmbCamera.SelectedItem as PTZCamera;
            UpdateCamInfo();
        }

        public void UpdateCamInfo()
        {
            if (lblCamInfo is null) { return; }

            string result = string.Empty;
            if (_camera is not null)
            {
                string auth = _camera.UseAuth ? ", auth" : "";
                result = $"{_camera.IP} ({_camera.ProtocolType.ToLowerString() + auth})";
            }
            lblCamInfo.Content = result;
        }

        public void ShowMessage(string text, MessageBoxImage mbi = MessageBoxImage.Information)
        {
            MessageBox.Show(this, text, "JW PTZ - AvA.Soft", MessageBoxButton.OK, mbi);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateSettingsFromUI();
            AppSettings.Save();
        }

        private void UpdateSettingsFromUI()
        {
            Settings.CommandTimeout = 2500;

        }

        private void sldPanSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZFSpeeds.PanSpeed = value;
            lblPanSpeed.Content = $"Pan speed ({value}):";
        }

        private void sldTiltSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZFSpeeds.TiltSpeed = value;
            lblTiltSpeed.Content = $"Tilt speed ({value}):";
        }

        private void sldZoomSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZFSpeeds.ZoomSpeed = value;
            lblZoomSpeed.Content = $"Zoom speed ({value}):";
        }

        private void sldFocusSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZFSpeeds.FocusSpeed = value;
            lblFocusSpeed.Content = $"Focus speed ({value}):";
        }

        private void ToggleSlimMode(object sender, RoutedEventArgs e)
        {
            // settings
            // update form size

        }

        private void btnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
