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
        public WinMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetView(ViewType.Main);
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

        private async Task TEST()
        {
            PTZCamera cam = new PTZCamera("192.168.0.88", ProtocolType.HTTP);
            PTZCommand cmd = PTZCommand.CallPresetInit(cam, 1);
            //cmd.CommandType = CommandType.None;
            var vvv = await APIs.SendCommand(cmd);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TEST();
        }
    }
}
