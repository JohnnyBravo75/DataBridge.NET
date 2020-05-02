namespace DataBridge.GUI.SpecialConverters
{
    using System;
    using System.Windows.Data;

    public class DatatypeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var dataType = value as Type;

            if (dataType == typeof(string))
            {
                return @"\Resources\Images\String_h24.png";
            }
            else if (dataType == typeof(Decimal)
                || dataType == typeof(UInt16)
                || dataType == typeof(UInt32)
                || dataType == typeof(UInt64)
                || dataType == typeof(Int16)
                || dataType == typeof(Int32)
                || dataType == typeof(Int64)
                || dataType == typeof(Double)
                || dataType == typeof(Single))
            {
                return @"\Resources\Images\Number_h24.png";
            }
            else if (dataType == typeof(DateTime))
            {
                return @"\Resources\Images\DateTime_h24.png";
            }

            return @"\Resources\Images\Object_h24.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}