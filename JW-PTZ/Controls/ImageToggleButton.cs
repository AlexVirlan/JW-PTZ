using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.Windows.Data;

namespace JWPTZ.Controls
{
    public class ImageToggleButton : ToggleButton
    {
        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ImageToggleButton),
                new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        public static readonly DependencyProperty UncheckedImageSourceProperty =
            DependencyProperty.Register(nameof(UncheckedImageSource), typeof(ImageSource), typeof(ImageToggleButton));

        public ImageSource UncheckedImageSource
        {
            get { return (ImageSource)GetValue(UncheckedImageSourceProperty); }
            set { SetValue(UncheckedImageSourceProperty, value); }
        }

        public static readonly DependencyProperty CheckedImageSourceProperty =
            DependencyProperty.Register(nameof(CheckedImageSource), typeof(ImageSource), typeof(ImageToggleButton));

        public ImageSource CheckedImageSource
        {
            get { return (ImageSource)GetValue(CheckedImageSourceProperty); }
            set { SetValue(CheckedImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageOpacityProperty =
            DependencyProperty.Register("ImageOpacity", typeof(double), typeof(ImageToggleButton), new PropertyMetadata(1.0));

        public double ImageOpacity
        {
            get { return (double)GetValue(ImageOpacityProperty); }
            set { SetValue(ImageOpacityProperty, value); }
        }

    }

    public class ToggleImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 &&
                values[0] is bool isChecked &&
                values[1] is ImageSource uncheckedSource &&
                values[2] is ImageSource checkedSource)
            {
                return isChecked ? checkedSource : uncheckedSource;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[]
            {
                false,
                DependencyProperty.UnsetValue,
                DependencyProperty.UnsetValue
            };
        }
    }

}
