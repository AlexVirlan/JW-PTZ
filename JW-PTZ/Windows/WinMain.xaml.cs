using JWPTZ.Controls;
using JWPTZ.Entities;
using JWPTZ.Services;
using JWPTZ.Utilities;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace JWPTZ.Windows
{
    public partial class WinMain : Window
    {
        #region Variables
        private bool _loading = false;
        private bool _internalChange = false;
        private int? _lastSelectedCameraId = null;
        public PTZCamera? _camera = null;
        private KeyboardHook? _keyboardHook;
        private BlurEffect _blurEffect = new BlurEffect();
        #endregion

        public WinMain()
        {
            _loading = true;
            this.Opacity = 0;
            InitializeComponent();
            HookCtrl();
        }

        private void HookCtrl()
        {
            _keyboardHook = new KeyboardHook([Key.LeftCtrl, Key.RightCtrl]);
            _keyboardHook.KeyboardPressed += OnCtrlPressed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SetView(ViewType.Main);

            MethodResponse fmrLoadSet = SettingsManager.Load();
            ApplySettingsToUI();
            UpdateCamInfoLabel();
            LoadPresetCacheImage();

            AnimateOpacity(from: 0, to: Settings.App.Opacity);
            AddUILog(UILogType.Info, "App started. Welcome! :)");

            //Settings.Cameras.CollectionChanged += CamerasOnCollectionChanged;

            _loading = false;
        }

        private void CamerasOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {

        }

        private void SetView(ViewType viewType)
        {
            //grdMain.Visibility = grdSettings.Visibility = Visibility.Hidden;
            //switch (viewType)
            //{
            //    case ViewType.Main: grdMain.Visibility = Visibility.Visible; break;
            //    case ViewType.Settings: grdSettings.Visibility = Visibility.Visible; break;
            //}
        }

        private void ApplySettingsToUI()
        {
            chkShowTimeStamp.IsChecked = Settings.UILogs.ShowTimestamp;
            chkAutoScrollUiLogs.IsChecked = Settings.UILogs.AutoScroll;
            chkIncludeParamsToUiLogs.IsChecked = Settings.UILogs.IncludeParams;
            chkShowFullEndpointToUiLogs.IsChecked = Settings.UILogs.ShowFullEndpoint;
            chkVerboseErrUiLogs.IsChecked = Settings.UILogs.VerboseErrors;

            sldPanSpeed.Value = Settings.PTZF.PanSpeed;
            sldTiltSpeed.Value = Settings.PTZF.TiltSpeed;
            sldZoomSpeed.Value = Settings.PTZF.ZoomSpeed;
            sldFocusSpeed.Value = Settings.PTZF.FocusSpeed;

            chkShowUILogs.IsChecked = Settings.UILogs.Visible;
            sldOpacity.Value = Settings.App.Opacity;
        }

        private void OnCtrlPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (_camera is null) { return; }

            if (e.KeyboardState == KeyboardHook.KeyboardState.KeyDown)
            {
                if (_camera.LockPresets)
                {
                    tabPresets.Header = "Presets (locked for this camera)";
                }
                else
                {
                    grdPresetsMain.Background = Helpers.GetBrushFromHex(Globals.DarkRedHex);
                    btnCallOtherPreset.Content = "Set";
                    tabPresets.Header = "Presets (SETTING)";
                }
            }
            else if (e.KeyboardState == KeyboardHook.KeyboardState.KeyUp)
            {
                grdPresetsMain.Background = Helpers.GetBrushFromHex(Globals.Gray26Hex);
                btnCallOtherPreset.Content = "Call";
                tabPresets.Header = "Presets (calling)";
            }
        }

        private void LoadPresetCacheImage()
        {
            //if (_camera is null) { return; }
            string dataCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Cache");

            string imgPath = string.Empty;
            for (int i = 1; i <= 15; i++)
            {
                Button? presetButton = this.FindName($"btnPreset{i}") as Button;
                if (presetButton is not null)
                {
                    imgPath = Path.Combine(dataCachePath, $"Cam{_camera?.Id}-Preset{i}.jpg");
                    if (_camera is not null && File.Exists(imgPath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imgPath, UriKind.Absolute);
                        //bitmap.CacheOption = BitmapCacheOption.OnLoad; // to be tested for bug 2
                        bitmap.EndInit();
                        //bitmap.Freeze(); // to be tested for bug 2
                        presetButton.Background = new ImageBrush(bitmap);
                    }
                    else
                    {
                        presetButton.Background = Helpers.GetBrushFromHex(Globals.Gray17Hex);
                    }
                }
            }
        }

        private void btnBackToMain_Click(object sender, RoutedEventArgs e)
        {
            //SetView(ViewType.Main);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowWindow(WindowType.Settings);
        }

        private void AddUILog(UILogType logType, string? text = null, PTZCommand? command = null, APIBaseResponse? response = null,
            UILogsSettings? uiLS = null)
        {
            if (logType == UILogType.Info && text.INOE()) { return; }
            if (logType == UILogType.Command && (command is null || response is null)) { return; }
            if (uiLS is null) { uiLS = Settings.UILogs; }

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
                    if (response.Successful) { rtbLogs.AppendFormattedText($"> ", brush: Helpers.GetBrushFromHex(Globals.GreenHex)); }
                    else { rtbLogs.AppendFormattedText($"> ", brush: Helpers.GetBrushFromHex(Globals.RedHex)); }

                    rtbLogs.AppendFormattedText($"Camera: ", brush: Helpers.GetBrushFromHex(Globals.Gray200Hex));
                    rtbLogs.AppendFormattedText($"{command.Camera.Id}. {command.Camera.Name} ", bold: true);
                    string camPath = uiLS.ShowFullEndpoint ? response.Endpoint : command.Camera.IP;
                    rtbLogs.AppendFormattedText($"({camPath}). Command: ", brush: Helpers.GetBrushFromHex(Globals.Gray200Hex));
                    rtbLogs.AppendFormattedText($"{command.CommandType}", bold: true);

                    if (uiLS.IncludeParams)
                    {
                        string qpStr = APIs.GetCommandParams(command);
                        rtbLogs.AppendFormattedText($"{(qpStr.INOE() ? "" : $" ({qpStr})")}", brush: Helpers.GetBrushFromHex(Globals.Gray200Hex));
                    }

                    rtbLogs.AppendFormattedText($". Result: ", brush: Helpers.GetBrushFromHex(Globals.Gray200Hex));
                    if (response.StatusCode is not null)
                    { rtbLogs.AppendFormattedText($"{response.StatusCode} ", bold: true); }

                    if (response.Successful) { rtbLogs.AppendFormattedText($"✔ Success!", brush: Helpers.GetBrushFromHex(Globals.GreenHex), appendNewLine: true); }
                    else
                    {
                        rtbLogs.AppendFormattedText($"✖ Fail!", brush: Helpers.GetBrushFromHex(Globals.RedHex), appendNewLine: true);
                        if (uiLS.VerboseErrors && !response.Message.INOE())
                        { rtbLogs.AppendFormattedText(response.Message, brush: Helpers.GetBrushFromHex(Globals.LightRedHex), appendNewLine: true); }
                    }
                    break;
            }
        }

        private void rtbLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Settings.UILogs.AutoScroll) { rtbLogs.ScrollToEnd(); }
        }

        private void UpdateUILogsSettings(object sender, RoutedEventArgs e)
        {
            if (_loading) { return; }
            Settings.UILogs.ShowTimestamp = chkShowTimeStamp.IsChecked();
            Settings.UILogs.AutoScroll = chkAutoScrollUiLogs.IsChecked();
            Settings.UILogs.IncludeParams = chkIncludeParamsToUiLogs.IsChecked();
            Settings.UILogs.ShowFullEndpoint = chkShowFullEndpointToUiLogs.IsChecked();
            Settings.UILogs.VerboseErrors = chkVerboseErrUiLogs.IsChecked();
        }

        private void lblClearUiLogs_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rtbLogs.Document.Blocks.Clear();
        }

        private async void CallPreset_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button? presetButton = null;
            try
            {
                #region Validations
                if (_camera is null)
                {
                    ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                    return;
                }

                presetButton = sender as Button;
                if (presetButton is null) { return; }

                if (!int.TryParse(presetButton.Name.Replace("btnPreset", "", StringComparison.OrdinalIgnoreCase), out int preset))
                { throw new Exception("Could not parse the preset button value."); }
                #endregion

                if (Settings.App.ButtonsWaitForResponse) { presetButton.IsEnabled = false; }
                bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (isCtrlPressed)
                    {
                        if (_camera.LockPresets)
                        {
                            ShowMessage("The presets for this camera are locked. " +
                                "You can change this for each camera individually, from this app's settings.");
                            return;
                        }

                        PTZCommand cmd = PTZCommand.SetPresetInit(_camera, preset);
                        APIBaseResponse presRes = await APIs.SendCommand(cmd);
                        AddUILog(UILogType.Command, null, cmd, presRes);

                        if (presRes.Successful && Settings.App.SnapshotOnSetPreset)
                        {
                            APIImageResponse imgRes = await APIs.GetSnapshot(_camera);
                            if (imgRes.Successful)
                            {
                                presetButton.Background = new ImageBrush(imgRes.BitmapImage) { Stretch = Stretch.Fill };
                                Helpers.SavePresetCacheImage(_camera.Id, preset, imgRes.BitmapImage);
                            }
                        }
                    }
                    else
                    {
                        PTZCommand cmd = PTZCommand.CallPresetInit(_camera, preset);
                        APIBaseResponse response = await APIs.SendCommand(cmd);
                        AddUILog(UILogType.Command, null, cmd, response);
                    }
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    // show image from preset
                }
                else if (e.ChangedButton == MouseButton.Middle)
                {
                    presetButton.Background = Helpers.GetBrushFromHex(Globals.Gray17Hex);
                    Helpers.DeletePresetCacheImage(_camera.Id, preset);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
            }
            finally
            {
                if (presetButton is not null) { presetButton.IsEnabled = true; }
            }
        }

        private async void PTZFOControl_PreviewMouseDownUp(object sender, MouseButtonEventArgs? e)
        {
            dynamic? button = null;
            string cmdName = string.Empty;
            try
            {
                #region Validations
                if (_camera is null)
                {
                    ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                    return;
                }

                if (sender is Button btn)
                {
                    button = btn;
                    cmdName = btn.Name.Replace("btn", "", StringComparison.OrdinalIgnoreCase);
                }
                else if (sender is ImageToggleButton itb)
                {
                    button = itb;
                    cmdName = itb.Tag.ToString() ?? string.Empty;
                }
                else
                {
                    ShowMessage("An error occurred while parsing the button data.", MessageBoxImage.Error);
                    return;
                }
                #endregion

                if (Settings.App.ButtonsWaitForResponse) { button.IsEnabled = false; }
                CommandType cmdType;

                if (_camera.OsdMode)
                {
                    if (cmdName.Contains("osd", "up", "down", "left", "right"))
                    {
                        cmdType = APIs.GetOSDCommand(cmdName);
                    }
                    else
                    {
                        if (e is not null && e.ButtonState == MouseButtonState.Pressed)
                        {
                            AddUILog(UILogType.Info, $"This command ({cmdName}) is invalid while the camera is in OSD mode.");
                        }
                        return;
                    }
                }
                else
                {
                    cmdType = Helpers.ParseEnum<CommandType>(value: cmdName, strict: false, fallback: CommandType.None);
                }

                if (e is not null && e.ButtonState == MouseButtonState.Released)
                {
                    cmdType = APIs.GetStopCommandType(cmdType);
                    if (cmdType == CommandType.None) { return; }
                }

                PTZCommand pTZCommand = new PTZCommand()
                {
                    Preset = 0,
                    Camera = _camera,
                    PTZFSpeeds = Settings.PTZF,
                    ImageSettings = Settings.Image,
                    CommandType = cmdType
                };

                APIBaseResponse response = await APIs.SendCommand(pTZCommand);
                AddUILog(UILogType.Command, null, pTZCommand, response);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
            }
            finally
            {
                if (button is not null) { button.IsEnabled = true; }
            }
        }

        private void cmbCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _internalChange = true;
                if (_camera is not null) { _camera.PropertyChanged -= Camera_PropertyChanged; }

                if (cmbCameras.SelectedItem is null)
                {
                    _camera = null;
                    itbOSD.IsChecked = false;
                    if (Settings.Cameras.Count == 0) { lblCamInfo.Content = "Go to settings to add cameras"; }
                    else { lblCamInfo.Content = "Please select a camera"; }
                    return;
                }

                _camera = cmbCameras.SelectedItem as PTZCamera;
                _lastSelectedCameraId = _camera?.Id ?? null;
                if (_camera is not null) { _camera.PropertyChanged += Camera_PropertyChanged; }
                itbOSD.IsChecked = _camera?.OsdMode ?? false;
                UpdateCamInfoLabel();
                LoadPresetCacheImage();
                SetPTZFOButtonsOpacity();
            }
            catch (Exception ex) { ShowMessage(ex.Message, MessageBoxImage.Error); }
            finally { _internalChange = false; }
        }

        private void Camera_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PTZCamera.IP) ||
                e.PropertyName == nameof(PTZCamera.ProtocolType) ||
                e.PropertyName == nameof(PTZCamera.UseAuth))
            {
                UpdateCamInfoLabel();
            }
        }

        private void UpdateCamInfoLabel()
        {
            if (lblCamInfo is null) { return; }

            string result = string.Empty;
            if (_camera is not null)
            {
                string auth = _camera.UseAuth ? ", auth" : "";
                result = $"{_camera.IP} ({_camera.ProtocolType.ToLowerString() + auth})";
            }
            else
            {
                if (Settings.Cameras.Count == 0) { result = "Go to settings to add cameras"; }
                else { result = "Please select a camera"; }
            }
            lblCamInfo.Content = result;
        }

        private void ShowMessage(string text, MessageBoxImage mbi = MessageBoxImage.Information)
        {
            MessageBox.Show(this, text, "JW PTZ - AvA.Soft", MessageBoxButton.OK, mbi);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateSettingsFromUI();
            SettingsManager.Save();
        }

        private void UpdateSettingsFromUI()
        {
            Settings.App.CommandTimeout = 2500;

        }

        private void sldPanSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZF.PanSpeed = value;
            lblPanSpeed.Content = $"Pan speed ({value}):";
        }

        private void sldTiltSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZF.TiltSpeed = value;
            lblTiltSpeed.Content = $"Tilt speed ({value}):";
        }

        private void sldZoomSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZF.ZoomSpeed = value;
            lblZoomSpeed.Content = $"Zoom speed ({value}):";
        }

        private void sldFocusSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            Settings.PTZF.FocusSpeed = value;
            lblFocusSpeed.Content = $"Focus speed ({value}):";
        }

        private void ToggleUILogs(object sender, RoutedEventArgs e)
        {
            if (_loading) { return; }
            Settings.UILogs.Visible = chkShowUILogs.IsChecked();
            if (Settings.UILogs.Visible)
            {
                grdLogs.Visibility = Visibility.Visible;
                this.Height += 190; // 690 + x
                //this.Height = 880;
            }
            else
            {
                grdLogs.Visibility = Visibility.Collapsed;
                this.Height -= 190; // 690 + x
                //this.Height = 690;
            }
        }

        private void btnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void btnCallOtherPreset_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                #region Validations
                if (_camera is null)
                {
                    ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                    return;
                }
                #endregion

                bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (isCtrlPressed)
                    {
                        PTZCommand cmd = PTZCommand.SetPresetInit(_camera, numOtherPreset.Value);
                        APIBaseResponse presRes = await APIs.SendCommand(cmd);
                        AddUILog(UILogType.Command, null, cmd, presRes);
                    }
                    else
                    {
                        PTZCommand cmd = PTZCommand.CallPresetInit(_camera, numOtherPreset.Value);
                        APIBaseResponse response = await APIs.SendCommand(cmd);
                        AddUILog(UILogType.Command, null, cmd, response);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void chkTakeSnapshots_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_loading) { return; }
            Settings.App.SnapshotOnSetPreset = chkTakeSnapshots.IsChecked();
        }

        private void btnResetPtzfSpeeds_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            sldPanSpeed.Value = 8;
            sldTiltSpeed.Value = 8;
            sldZoomSpeed.Value = 3;
            sldFocusSpeed.Value = 3;
            CloseMenuPopup();
        }

        private void OsdEnterBack_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            #region Validations
            if (_camera is null)
            {
                ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                return;
            }
            #endregion

            if (_camera.OsdMode)
            { PTZFOControl_PreviewMouseDownUp(sender, e); }
            else
            { AddUILog(UILogType.Info, "The current camera is not in OSD mode. Please enable this mode before using the OSD buttons."); }
        }

        private void itbOSD_CheckedChanged(object sender, RoutedEventArgs e)
        {
            #region Validations
            if (_internalChange) { return; }

            if (_camera is null)
            {
                ShowMessage("Please select a camera first.", MessageBoxImage.Warning);
                return;
            }

            ImageToggleButton? itb = sender as ImageToggleButton;
            if (itb is null)
            {
                ShowMessage("An error occurred while parsing the OSD button data.", MessageBoxImage.Error);
                return;
            }
            #endregion

            _camera.OsdMode = itbOSD.IsChecked ?? false;
            itb.Tag = _camera.OsdMode ? "OsdOn" : "OsdOff";
            PTZFOControl_PreviewMouseDownUp(itb, null);

            SetPTZFOButtonsOpacity();
        }

        private void SetPTZFOButtonsOpacity()
        {
            bool osdMode = _camera is not null && _camera.OsdMode;

            btnOsdEnter.Opacity = btnOsdBack.Opacity = (osdMode ? 1 : 0.26);
            btnPanLeftTiltUp.Opacity = btnPanRightTiltUp.Opacity = btnGoHome.Opacity = btnPanLeftTiltDown.Opacity =
                btnPanRightTiltDown.Opacity = btnZoomIn.Opacity = btnZoomOut.Opacity = btnFocusIn.Opacity =
                btnFocusOut.Opacity = btnActivateAutoFocus.Opacity = (osdMode ? 0.26 : 1);
        }

        private void btnCallOtherPreset_MouseEnter(object sender, MouseEventArgs e)
        {
            btnCallOtherPreset.Focus();
        }

        private void sldOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) { return; }
            double sldVal = Math.Round(e.NewValue, 2);
            this.Opacity = Settings.App.Opacity = sldVal;
            lblOpacity.Content = $"Opacity ({Math.Round(sldVal * 100, 0)}%):";
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) { ToggleWindowState(); }
            else { this.DragMove(); }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleWindowState()
        {
            //this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            chkShowUILogs.Toggle();
        }

        private dynamic? ShowWindow(WindowType windowType)
        {
            dynamic? result = null;
            try
            {
                if (Settings.App.Opacity > 0.64) { AnimateOpacity(from: Settings.App.Opacity, to: 0.64); }
                AnimateBlur(true);

                switch (windowType)
                {
                    case WindowType.Settings:
                        WinSettings winSet = new(ownerWindow: this);
                        winSet.Owner = this;
                        winSet.ShowDialog();
                        result = winSet.Data;
                        break;

                    case WindowType.Sync:

                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
                return null;
            }
            finally
            {
                if (Settings.App.Opacity > 0.64) { AnimateOpacity(from: 0.64, to: Settings.App.Opacity, durationMs: 460); }
                AnimateBlur(false);
            }
        }

        private void AnimateOpacity(double from, double to, int durationMs = 1000)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                FillBehavior = FillBehavior.Stop
            };
            opacityAnimation.Completed += (s, e) => { this.Opacity = to; };
            this.BeginAnimation(Window.OpacityProperty, opacityAnimation);
        }

        private void AnimateBlur(bool enabled)
        {
            this.Effect = _blurEffect;
            DoubleAnimation blurAnimation = new DoubleAnimation
            {
                From = (enabled ? 0 : 10),
                To = (enabled ? 10 : 0),
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.Stop
            };
            blurAnimation.Completed += (s, e) => { this._blurEffect.Radius = (double)blurAnimation.To; };
            _blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
        }

        private void CloseMenuPopup()
        {
            puMenu.IsPopupOpen = false;
        }

        private void puTools_MouseEnter(object sender, MouseEventArgs e)
        {
            puTools.IsPopupOpen = true;
        }

        private void puTools_MouseLeave(object sender, MouseEventArgs e)
        {
            //if (puTools != null)
            //{
            //    Point position = e.GetPosition(puTools);
            //    HitTestResult hitTest = VisualTreeHelper.HitTest(puTools, position);
            //    if (hitTest == null) { puTools.IsPopupOpen = false; }
            //}
        }

        private void puToolsPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            //puTools.IsPopupOpen = false;
            if (puTools is not null)
            {
                Point position = e.GetPosition(puTools);
                HitTestResult hitTest = VisualTreeHelper.HitTest(puTools, position);
                if (hitTest == null) { puTools.IsPopupOpen = false; }
            }
        }

        private void btnTools_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void btnTools_MouseLeave(object sender, MouseEventArgs e)
        {
            if (puToolsStackPanel is not null)
            {
                Point position = e.GetPosition(puToolsStackPanel);
                position.X -= 10;
                HitTestResult hitTest = VisualTreeHelper.HitTest(puToolsStackPanel, position);
                if (hitTest == null) { puTools.IsPopupOpen = false; }
            }
        }

        private void btnFullPresets_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            //puMenu.IsPopupOpen = true;

        }

        public void RestoreLastSelectedCamera()
        {
            if (_lastSelectedCameraId is not null)
            {
                PTZCamera? lastCam = Settings.Cameras.FirstOrDefault(cam => cam.Id == _lastSelectedCameraId);
                if (lastCam is not null) { cmbCameras.SelectedItem = lastCam; }
            }

            //if (Settings.Cameras.Count > 0 &&
            //    cmbCameras.SelectedItem is null &&
            //    _lastSelectedCameraId is not null)
            //{
            //    PTZCamera? camera = Settings.Cameras.FirstOrDefault(c => c.Id == _lastSelectedCameraId);
            //    int cameraIndex = camera is not null ? Settings.Cameras.IndexOf(camera) : -1;
            //    cmbCameras.SelectedIndex = cameraIndex > -1 ? cameraIndex : 0;
            //}
        }
    }
}
