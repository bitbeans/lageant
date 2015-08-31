using System;
using System.Globalization;
using System.Windows.Data;
using lageant.client.Models;
using Sodium;

namespace SimpleCrypt.Converter
{
    [ValueConversion(typeof (Key), typeof (string))]
    public class KeyToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    var key = (Key) value;
                    if (key.KeyId != null)
                    {
                        return string.Format("{0} ({1})",
                            Utilities.BinaryToHex(key.KeyId, Utilities.HexFormat.Colon, Utilities.HexCase.Upper),
                            key.KeyType);
                    }
                    return string.Format("{0} ({1})",
                        Utilities.BinaryToHex(key.PublicKey, Utilities.HexFormat.Colon, Utilities.HexCase.Upper),
                        key.KeyType);
                }
                catch (Exception)
                {
                    return value;
                }
            }
            return "<bad key>";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}