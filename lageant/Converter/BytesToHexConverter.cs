using System;
using System.Globalization;
using System.Windows.Data;
using Sodium;

namespace lageant.Converter
{
    /// <summary>
    /// Convert a byte array into a viewable hex string.
    /// </summary>
    public class BytesToHexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = (byte[])values[0];
            var type = (string)values[1];

            try
            {
                if (bytes != null)
                {
                    var hex = Utilities.BinaryToHex((byte[]) bytes, Utilities.HexFormat.Colon, Utilities.HexCase.Upper);
                    switch (type) 
                    {
                        case "id":
                            return string.Format("{0}", hex);
                        case "public":
                            return string.Format("PublicKey: {0} [{1} bytes]", hex, bytes.Length);
                        case "private":
                            return string.Format("PrivateKey: {0} [{1} bytes]", hex, bytes.Length);
                        default:
                            return string.Format("{0}", hex);
                    }
                }
                switch (type)
                {
                    case "id":
                        return "no key id";
                    case "public":
                        return "no public key";
                    case "private":
                        return "no private key";
                    default:
                        return "no data";
                }
            }
            catch
            {
                return "could not convert";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}