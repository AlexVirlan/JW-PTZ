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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JWPTZ.Controls
{
    public partial class TogglePasswordBox : UserControl
    {
        private bool _isPasswordVisible = false;
        private bool _isUpdating = false;

        public TogglePasswordBox()
        {
            InitializeComponent();
        }

        public string Password
        {
            get => passwordBox.Password;
            set => passwordBox.Password = value;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                textBox.Text = passwordBox.Password;
                textBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                toggleButton.Content = "🙈";
                toggleButton.ToolTip = "Hide password";
            }
            else
            {
                passwordBox.Password = textBox.Text;
                passwordBox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;
                toggleButton.Content = "👁";
                toggleButton.ToolTip = "Show password";
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isUpdating)
            {
                _isUpdating = true;
                textBox.Text = passwordBox.Password;
                _isUpdating = false;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdating)
            {
                _isUpdating = true;
                passwordBox.Password = textBox.Text;
                _isUpdating = false;
            }
        }
    }
}
