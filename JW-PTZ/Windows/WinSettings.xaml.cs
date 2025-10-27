using JWPTZ.Entities;
using JWPTZ.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace JWPTZ.Windows
{
    public class BackupSettingsNotifier : Freezable, INotifyPropertyChanged
    {
        protected override Freezable CreateInstanceCore() => new BackupSettingsNotifier();

        private ObservableCollection<PTZCamera> _cameras;

        public BackupSettingsNotifier()
        {
            _cameras = new ObservableCollection<PTZCamera>();
        }

        public ObservableCollection<PTZCamera> Cameras
        {
            get => _cameras;
            set { _cameras = value; OnPropertyChanged(nameof(Cameras)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class WinSettings : Window
    {
        #region Variables
        public object? Data { get; set; }

        private bool _loading = false;
        private readonly string _NL = Environment.NewLine;
        private PTZCamera? _selectedCamera = null;
        private BackupSettingsNotifier _backupSettingsNotifier = new();
        private Window? _ownerWindow = null;
        #endregion

        public WinSettings(Window? ownerWindow)
        {
            _loading = true;

            _ownerWindow = ownerWindow;
            InitializeComponent();

            _backupSettingsNotifier.Cameras = CreateDeepCopy(Settings.Cameras);
            this.DataContext = _backupSettingsNotifier;
        }

        private ObservableCollection<PTZCamera> CreateDeepCopy(ObservableCollection<PTZCamera> source)
        {
            ObservableCollection<PTZCamera> copy = [];
            foreach (PTZCamera camera in source)
            { copy.Add(camera.DeepCopy()); }
            return copy;
        }

        private void OnCamerasChanged()
        {
            //Settings.ReindexCameras();
        }

        private void LoadSelectedCameraDetails()
        {
            if (_loading) { return; }
            _loading = true;
            _selectedCamera = lstCameras.SelectedItem as PTZCamera;
            lblCamId.Content = "Camera ID: " + (_selectedCamera is not null ? $"{_selectedCamera?.Id}" : "-");
            txtCamName.Text = _selectedCamera?.Name ?? string.Empty;
            txtCamIp.Text = _selectedCamera?.IP ?? string.Empty;
            chkUseAuthentication.IsChecked = _selectedCamera?.UseAuth ?? false;
            txtAuthUser.Text = _selectedCamera?.Username ?? string.Empty;
            pwbAuthPass.Password = _selectedCamera?.Password ?? string.Empty;
            cmbProtocol.SelectedIndex = _selectedCamera is null ? -1 : _selectedCamera.ProtocolType == ProtocolType.HTTP ? 0 : 1;
            chkLockPresets.IsChecked = _selectedCamera?.LockPresets ?? false;
            _loading = false;
        }

        private void lstCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSelectedCameraDetails();
            
        }

        private void chkUseAuthentication_CheckedChanged(object sender, RoutedEventArgs e)
        {
            txtAuthUser.IsEnabled = txtAuthPass.IsEnabled = pwbAuthPass.IsEnabled = chkUseAuthentication.IsChecked();
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.UseAuth = chkUseAuthentication.IsChecked();
        }

        private void lblShowAuthPass_PreviewMouseDownUp(object sender, MouseButtonEventArgs e)
        {
            bool isPressed = e.ButtonState == MouseButtonState.Pressed;
            txtAuthPass.Text = isPressed ? pwbAuthPass.Password : string.Empty;
            pwbAuthPass.Visibility = isPressed ? Visibility.Collapsed : Visibility.Visible;
            txtAuthPass.Visibility = isPressed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chkLockPresets_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.LockPresets = chkLockPresets.IsChecked();
        }

        private void chkAutoSave_CheckedChanged(object sender, RoutedEventArgs e)
        {
            btnSave.SetEnabled(!chkAutoSave.IsChecked());
            Settings.App.AutoSaveCamsSettings = chkAutoSave.IsChecked();
        }

        private void txtCamIp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.IP = txtCamIp.Text.Trim();
            txtCamIp.Foreground = Helpers.IsValidIPv4(txtCamIp.Text.Trim()) ? Globals.GreenText : Globals.RedText;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _backupSettingsNotifier.Cameras.CollectionChanged += CamerasOnCollectionChanged;
            chkAutoSave.IsChecked = Settings.App.AutoSaveCamsSettings;
            _loading = false;
            LoadSelectedCameraDetails();
            UpdateCamerasCountLabel();
        }

        private void CamerasOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateCamerasCountLabel();
        }

        private void ShowMessage(string message, MessageBoxImage mbi = MessageBoxImage.Information)
        {
            MessageBox.Show(this, message, "JW PTZ - Settings", MessageBoxButton.OK, mbi);
        }

        private MessageBoxResult AskUser(string message)
        {
            return MessageBox.Show(this, message, "JW PTZ - Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddCam_Click(object sender, RoutedEventArgs e)
        {
            //_backupSettingsNotifier.Cameras.Add(new PTZCamera() { Name = "xXxXx", IP = "123.456" });

            int newId = GetNextCamraId();
            _backupSettingsNotifier.Cameras.Add(new PTZCamera(newId, $"New camera {newId}"));
            lstCameras.SelectedIndex = lstCameras.Items.Count - 1;
        }

        private void brnDuplicateCam_Click(object sender, RoutedEventArgs e)
        {
            #region Validations
            if (lstCameras.Items.Count == 0)
            {
                ShowMessage("There are no cameras in the list.");
                return;
            }

            if (_selectedCamera is null)
            {
                ShowMessage("Please select a camera first.");
                return;
            }
            #endregion

            PTZCamera clonedCamera = new()
            {
                Id = GetNextCamraId(),
                IP = _selectedCamera.IP,
                Name = $"{_selectedCamera.Name} (duplicate)",
                UseAuth = _selectedCamera.UseAuth,
                Username = _selectedCamera.Username,
                Password = _selectedCamera.Password,
                LockPresets = _selectedCamera.LockPresets,
                OsdMode = false,
                ProtocolType = _selectedCamera.ProtocolType
            };
            _backupSettingsNotifier.Cameras.Add(clonedCamera);
            lstCameras.SelectedIndex = lstCameras.Items.Count - 1;
        }

        private void btnMoveCamUp_Click(object sender, RoutedEventArgs e)
        {
            #region Validations
            if (lstCameras.Items.Count == 0)
            {
                ShowMessage("There are no cameras in the list.");
                return;
            }

            if (_selectedCamera is null)
            {
                ShowMessage("Please select a camera first.");
                return;
            }
            #endregion

            int index = _backupSettingsNotifier.Cameras.IndexOf(_selectedCamera);
            if (index == 0) { return; }

            if (index < 0)
            {
                ShowMessage("Error moving the selected camera.", MessageBoxImage.Error);
                return;
            }

            _backupSettingsNotifier.Cameras.Move(index, index - 1);
            lstCameras.SelectedIndex = index - 1;
            ReindexCameras();
        }

        private void btnMoveCamDown_Click(object sender, RoutedEventArgs e)
        {
            #region Validations
            if (lstCameras.Items.Count == 0)
            {
                ShowMessage("There are no cameras in the list.");
                return;
            }

            if (_selectedCamera is null)
            {
                ShowMessage("Please select a camera first.");
                return;
            }
            #endregion

            int index = _backupSettingsNotifier.Cameras.IndexOf(_selectedCamera);
            if (index >= _backupSettingsNotifier.Cameras.Count - 1) { return; }

            if (index < 0)
            {
                ShowMessage("Error moving the selected camera.", MessageBoxImage.Error);
                return;
            }

            _backupSettingsNotifier.Cameras.Move(index, index + 1);
            lstCameras.SelectedIndex = index + 1;
            ReindexCameras();
        }

        private void btnDeleteCam_Click(object sender, RoutedEventArgs e)
        {
            if (lstCameras.Items.Count == 0)
            {
                ShowMessage("There are no cameras in the list.");
                return;
            }

            if (_selectedCamera is null)
            {
                ShowMessage("Please select a camera first.");
                return;
            }

            MessageBoxResult result = AskUser($"Are you sure you want to delete this camera:{_NL}Name: {_selectedCamera.Name}{_NL}IP: {_selectedCamera.IP}");
            if (result == MessageBoxResult.Yes)
            {
                bool notLastCam = lstCameras.SelectedIndex < lstCameras.Items.Count - 1;
                bool removed = _backupSettingsNotifier.Cameras.Remove(_selectedCamera);
                if (removed)
                {
                    if (notLastCam) { ReindexCameras(); }
                }
                else
                {
                    ShowMessage("Error deleting the selected camera.", MessageBoxImage.Error);
                }
            }
        }

        private void btnDeleteAllCam_Click(object sender, RoutedEventArgs e)
        {
            if (lstCameras.Items.Count == 0)
            {
                ShowMessage("There are no cameras in the list.");
                return;
            }

            MessageBoxResult result = AskUser($"Are you sure you want to delete all cameras?");
            if (result == MessageBoxResult.Yes)
            {
                _backupSettingsNotifier.Cameras.Clear();
            }
        }

        private async void btnPingCamIp_Click(object sender, RoutedEventArgs e)
        {
            string destination = txtCamIp.Text.Trim();
            string destinationMsg = $"Destination: {destination + _NL}";
            try
            {
                if (!Helpers.IsValidIPv4(destination))
                {
                    ShowMessage("Invalid camera IP address or hostname.", MessageBoxImage.Warning);
                    return;
                }

                btnPingCamIp.SetEnabled(false);
                Ping ping = new();
                PingReply pingReply = await ping.SendPingAsync(destination, 3000);

                if (pingReply.Status == IPStatus.Success)
                { ShowMessage($"Ping successful!{_NL.Repeat()}{destinationMsg}Roundtrip time: {pingReply.RoundtripTime} ms", MessageBoxImage.Information); }
                else
                { ShowMessage($"Ping failed!{_NL.Repeat()}{destinationMsg}Status: {pingReply.Status}", MessageBoxImage.Warning); }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error pinging camera.{_NL.Repeat()}{destinationMsg}Details:{_NL + ex.Message}", MessageBoxImage.Error);
            }
            finally
            {
                btnPingCamIp.SetEnabled(true);
            }
        }

        private void btnOpenCamIp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (bool isValid, string message) = Helpers.CheckIfNullOrEmpty(
                    ("Camera IP (or hostname)", txtCamIp.Text.Trim()),
                    ("Protocol", cmbProtocol.Text));
                if (!isValid)
                {
                    ShowMessage($"Cannot open camera IP/hostname.{_NL}Missing or invalid parameters: {_NL + message}", MessageBoxImage.Warning);
                    return;
                }

                string url = $"{cmbProtocol.Text}://{txtCamIp.Text.Trim()}/";
                ProcessStartInfo psi = new() { FileName = url, UseShellExecute = true };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                ShowMessage($"Error opening camera IP/hostname.{_NL}Details:{_NL}{ex.Message}", MessageBoxImage.Error);
            }
        }

        private void lblCamIdInfo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowMessage("The Camera ID is a unique identifier assigned automatically to each camera in the list. It is used internally by the application to differentiate between cameras, especially when saving and loading settings. It's also used in the preset snapshots as part of the image filename. The Camera ID cannot be changed by you (the user) within the JW PTZ app. Manually changing this ID from the app's settings file can cause unwanted behavior or errors in the app, so please don't do that.");
        }

        private void lblAutoSaveInfo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowMessage($"When enabled, the application will automatically save any changes made to the camera settings immediately after they are made. This means that you don't have to manually save your settings each time you make a change; the app will handle it for you. This feature is useful for ensuring that all your configurations are preserved without the need for extra steps, reducing the risk of losing changes if you forget to save manually.{_NL.Repeat()}Note: The state of this setting (enabled/disabled) will be saved and remembered for the next time.");
        }

        private void txtCamName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.Name = txtCamName.Text.Trim();
        }

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                MessageBoxResult result = AskUser("Are you sure you want to revert all camera settings (for all cameras) to the last saved state?");
                if (result == MessageBoxResult.Yes)
                {
                    _backupSettingsNotifier.Cameras.CollectionChanged -= CamerasOnCollectionChanged;
                    _backupSettingsNotifier.Cameras.Clear();
                    _backupSettingsNotifier.Cameras = CreateDeepCopy(Settings.Cameras);
                    UpdateCamerasCountLabel();
                    _backupSettingsNotifier.Cameras.CollectionChanged += CamerasOnCollectionChanged;
                }
            }
            catch (Exception ex)
            { ShowMessage($"Error reverting camera settings.{_NL}{ex.Message}", MessageBoxImage.Error); }
            finally
            { this.IsEnabled = true; }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                SaveCamerasSettings();
                //SaveCamerasSettings_V2();
            }
            catch (Exception ex)
            { ShowMessage($"Error saving camera settings.{_NL}{ex.Message}", MessageBoxImage.Error); }
            finally
            { this.IsEnabled = true; }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Settings.App.AutoSaveCamsSettings) { SaveCamerasSettings(); }

        }

        private void SaveCamerasSettings()
        {
            Settings.Cameras.Clear();
            foreach (PTZCamera camera in _backupSettingsNotifier.Cameras)
            { Settings.Cameras.Add(camera.DeepCopy()); }
            SettingsManager.Save();
            (_ownerWindow as WinMain)?.RestoreLastSelectedCamera();
        }

        public void SaveCamerasSettings_V2()
        {
            var backupCamerasById = _backupSettingsNotifier.Cameras.ToDictionary(c => c.Id);
            var originalCamerasById = Settings.Cameras.ToDictionary(c => c.Id);

            for (int i = Settings.Cameras.Count - 1; i >= 0; i--)
            {
                if (!backupCamerasById.ContainsKey(Settings.Cameras[i].Id))
                {
                    Settings.Cameras.RemoveAt(i);
                }
            }

            for (int i = 0; i < _backupSettingsNotifier.Cameras.Count; i++)
            {
                var backupCamera = _backupSettingsNotifier.Cameras[i];

                if (originalCamerasById.TryGetValue(backupCamera.Id, out var originalCamera))
                {
                    originalCamera.IP = backupCamera.IP;
                    originalCamera.Name = backupCamera.Name;
                    originalCamera.UseAuth = backupCamera.UseAuth;
                    originalCamera.Username = backupCamera.Username;
                    originalCamera.Password = backupCamera.Password;
                    originalCamera.LockPresets = backupCamera.LockPresets;
                    originalCamera.OsdMode = backupCamera.OsdMode;
                    originalCamera.ProtocolType = backupCamera.ProtocolType;

                    int currentIndex = Settings.Cameras.IndexOf(originalCamera);
                    if (currentIndex != i)
                    {
                        Settings.Cameras.Move(currentIndex, i);
                    }
                }
                else
                {
                    Settings.Cameras.Insert(i, (PTZCamera)backupCamera.Clone());
                }
            }

            SettingsManager.Save();
        }

        private void txtAuthUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.Username = txtAuthUser.Text.Trim();
        }

        private void pwbAuthPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.Password = pwbAuthPass.Password;
        }

        private void cmbProtocol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }
            _selectedCamera.ProtocolType = cmbProtocol.SelectedIndex == 0 ? ProtocolType.HTTP : ProtocolType.HTTPS;
        }

        private int GetNextCamraId()
        {
            if (_backupSettingsNotifier.Cameras.Count == 0) { return 1; }
            return _backupSettingsNotifier.Cameras.Max(cam => cam.Id) + 1;
            //return _backupSettingsNotifier.Cameras.Count + 1;
        }

        private void ReindexCameras()
        {
            for (int i = 0; i < _backupSettingsNotifier.Cameras.Count; i++)
            {
                _backupSettingsNotifier.Cameras[i].Id = i + 1;
            }
        }

        private void UpdateCamerasCountLabel()
        {
            if (_loading) { return; }
            lblCamsCount.Content = $"Cameras ({_backupSettingsNotifier.Cameras.Count}):";
        }

        private void LblClick_MouseEnter(object sender, MouseEventArgs e)
        {
            Label? lbl = sender as Label;
            if (lbl is not null) { lbl.Foreground = new SolidColorBrush(Colors.White); }
        }

        private void LblClick_MouseLeave(object sender, MouseEventArgs e)
        {
            Label? lbl = sender as Label;
            if (lbl is not null) { lbl.Foreground = Globals.GrayText; }
        }
    }
}
