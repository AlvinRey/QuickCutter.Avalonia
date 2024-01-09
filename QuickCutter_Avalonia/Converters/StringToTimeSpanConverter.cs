using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Converters
{
    public class StringToTimeSpanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {

            if (value is TimeSpan timeSpanValue)
            {
                return timeSpanValue.ToString(@"hh\:mm\:ss\.ff");
            }

            return string.Empty; // 如果无法转换，返回空字符串或者根据需求返回其他值
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (TimeSpan.TryParseExact(stringValue, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture, out TimeSpan timeSpan))
                {
                    return timeSpan;
                }
            }

            return TimeSpan.Zero;
        }
    }
}
