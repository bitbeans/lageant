using System;
using System.Globalization;
using System.Windows.Data;
using lageant.core.Models;

namespace lageant.Converter
{
    [ValueConversion(typeof (KeyType), typeof (string))]
    public class KeyTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((KeyType) value)
            {
                case KeyType.Bytejail:
                    return "../Images/bytejail.ico";
                case KeyType.Lageant:
                    return "../Images/libsodium.ico";
                case KeyType.Curvelock:
                    return "../Images/curvelock.ico";
                case KeyType.Minilock:
                    return "../Images/minilock.ico";
                case KeyType.Minisign:
                    return "../Images/libsodium.ico";
                default:
                    return "../Images/libsodium.ico";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}