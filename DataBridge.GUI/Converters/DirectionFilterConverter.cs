namespace DataBridge.GUI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using DataBridge;

    public class DirectionFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var all = value as IEnumerable<CommandParameter>;
            if (all == null) return null;
            var filter = (parameter as string ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(filter)) return all.ToList();
            var allowed = filter.Split(',').Select(s => s.Trim()).ToList();
            return all.Where(p => allowed.Contains(p.Direction.ToString())).ToList();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
