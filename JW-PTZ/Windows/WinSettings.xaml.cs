using JWPTZ.Utilities;
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

namespace JWPTZ.Windows
{
    public partial class WinSettings : Window
    {
        public object? Data { get; set; }

        public WinSettings()
        {
            InitializeComponent();
            Settings.CamerasChanged += (s, e) => OnCamerasChanged();
        }

        private void OnCamerasChanged()
        {
            Settings.ReindexCameras();
        }

        private void lstCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnAddCam_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void brnDuplicateCam_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnMoveCamUp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnMoveCamDown_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnDeleteCam_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnDeleteAllCam_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void lblCamIdInfo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnPingCamIp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnOpenCamIp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void chkUseAuthentication_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void lblShowAuthPass_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void chkLockPresets_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void lblAutoSaveInfo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void chkAutoSave_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
