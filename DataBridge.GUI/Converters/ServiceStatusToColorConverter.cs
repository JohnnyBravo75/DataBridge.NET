using System.ServiceProcess;
using System.Windows.Media;

namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows.Data;

    public class ServiceStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ServiceControllerStatus)
            {
                var status = (ServiceControllerStatus)value;
                if (status == ServiceControllerStatus.Running)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 142, 196, 65));
                }
            }

            return new SolidColorBrush(Color.FromArgb(255, 227, 60, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}