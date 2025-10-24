using JWPTZ.Entities;
using JWPTZ.Utilities;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace JWPTZ.Windows
{
    public partial class WinSettings : Window
    {
        #region Variables
        public object? Data { get; set; }

        private bool _loading = false;
        private readonly string _NL = Environment.NewLine;
        private PTZCamera? _selectedCamera = null;
        #endregion

        public WinSettings()
        {
            _loading = true;
            InitializeComponent();
            Settings.CamerasChanged += (s, e) => OnCamerasChanged();
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

        }

        private void chkAutoSave_CheckedChanged(object sender, RoutedEventArgs e)
        {
            btnSave.SetEnabled(!chkAutoSave.IsChecked());
        }

        private void txtCamIp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading || _selectedCamera is null) { return; }

            txtCamIp.Foreground = Helpers.IsValidIPv4(txtCamIp.Text.Trim()) ? Globals.GreenText : Globals.RedText;

            Settings.Cameras[lstCameras.SelectedIndex].IP = txtCamIp.Text.Trim();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chkAutoSave.IsChecked = Settings.App.AutoSaveCamsSettings;
            _loading = false;
            LoadSelectedCameraDetails();
            lblCamsTitle.Content = $"Cameras ({Settings.Cameras.Count}):";
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
            Settings.Cameras.Add(new PTZCamera() { Name = "xXxXx", IP = "123.456" });
        }

        private void brnDuplicateCam_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveCamUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveCamDown_Click(object sender, RoutedEventArgs e)
        {

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

            }
        }

        private void btnDeleteAllCam_Click(object sender, RoutedEventArgs e)
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

            MessageBoxResult result = AskUser($"Are you sure you want to delete all cameras?");
            if (result == MessageBoxResult.Yes)
            {

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
            //_selectedCamera.Name = txtCamName.Text.Trim();
            Settings.Cameras[lstCameras.SelectedIndex].Name = txtCamName.Text.Trim();
        }
    }
}
