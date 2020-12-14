using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace WPFLab.LightPropertyGrid {
    public class VisibleModeToVisibilityConvertor : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is VisibleModes vis) {
                switch (vis) {
                    case VisibleModes.Collapsed:
                        return Visibility.Collapsed;
                    case VisibleModes.Hidden:
                        return Visibility.Hidden;
                    case VisibleModes.Visible:
                        return Visibility.Visible;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
