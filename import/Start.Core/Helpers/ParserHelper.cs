using Microsoft.Azure.Cosmos.Spatial;
using System;
using System.Globalization;

namespace Start.Core.Helpers
{
    /// <summary>
    /// Outils de conversion de chaîne en type
    /// </summary>
    public static class ParserHelper
    {
        /// <summary>
        /// Convertit une chaîne en double avec un '.'
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double ParseDouble(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return default;
            }

            char sepdec = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (double.TryParse(str.Replace(",", sepdec.ToString()).Replace(".", sepdec.ToString()), out double res))
            {
                return res;
            }

            return default;
        }

        public static string ConvertToString(Point location)
        {
            var longitude = location.Position.Longitude.ToString().Replace(",", ".");
            var latitude = location.Position.Latitude.ToString().Replace(",", ".");
            return $"{latitude},{longitude}";
        }
    }
}
