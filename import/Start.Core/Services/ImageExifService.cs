using Start.Core.Entities;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    /// <summary>
    /// Service de récupération des métadonnées d'une image
    /// </summary>
    public class ImageExifService
    {
        private const string _imageServerPath = "https://start-prod.fr/photos/br/";
        private readonly string[] _imageExtensions = new[] { ".jpg", ".jpeg" };

        /// <summary>
        /// Obtient les coordonnées spatiales de géolocalisation contenues dans l'image d'une oeuvre
        /// </summary>
        /// <param name="oeuvre">L'oeuvre</param>
        /// <returns>Les coordonnées spatiales de l'oeuvre</returns>
        public async Task<Microsoft.Azure.Cosmos.Spatial.Point> GetOeuvreGeolocalisation(Oeuvre oeuvre)
        {
            Microsoft.Azure.Cosmos.Spatial.Point position = null;
            var url = oeuvre.ImageUrl;
            var i = 0;

            using (var client = new HttpClient())
            {
                while (true)
                {
                    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        using var result = await client.GetAsync(url);
                        if (result.IsSuccessStatusCode)
                        {
                            using var ms = new MemoryStream(await result.Content.ReadAsByteArrayAsync());
                            position = GetImageGeolocalisation(ms);
                            oeuvre.ImageUrl = url;

                            break;
                        }
                    }

                    url = string.Concat(_imageServerPath, oeuvre.Marqueur, _imageExtensions[i]);
                    i++;

                    if (i == _imageExtensions.Length)
                    {
                        break;
                    }
                }
            }

            return position;
        }

        /// <summary>
        /// Obtient les coordonnées spatiales contenues dans les métadonnées d'une image
        /// </summary>
        /// <param name="stream">Le flux de l'image</param>
        /// <returns>les coordonnées spatiales contenues dans les métadonnées d'une image</returns>
        public Microsoft.Azure.Cosmos.Spatial.Point GetImageGeolocalisation(Stream stream)
        {
            Microsoft.Azure.Cosmos.Spatial.Point position = null;
            try
            {
                using (var bitmap = new Bitmap(stream))
                {
                    if (bitmap.PropertyItems.Where(p => p.Id == 4 || p.Id == 2).Count() == 2)
                    {
                        var longitude = GetCoordinateDouble(bitmap.PropertyItems.Single(p => p.Id == 3).Value[0], bitmap.PropertyItems.Single(p => p.Id == 4));
                        var latitude = GetCoordinateDouble(bitmap.PropertyItems.Single(p => p.Id == 1).Value[0], bitmap.PropertyItems.Single(p => p.Id == 2));

                        position = new Microsoft.Azure.Cosmos.Spatial.Point(longitude, latitude);
                    }
                }
            }
            catch
            {

            }

            return position;
        }

        /// <summary>
        /// Obtient les coordonnées contenues dans les métadonnées d'une image
        /// </summary>
        /// <param name="propertyItem"></param>
        /// <returns></returns>
        private double GetCoordinateDouble(byte gpsRef, PropertyItem propertyItem)
        {
            uint degreesNumerator = BitConverter.ToUInt32(propertyItem.Value, 0);
            uint degreesDenominator = BitConverter.ToUInt32(propertyItem.Value, 4);
            double degrees = degreesNumerator / (double)degreesDenominator;

            uint minutesNumerator = BitConverter.ToUInt32(propertyItem.Value, 8);
            uint minutesDenominator = BitConverter.ToUInt32(propertyItem.Value, 12);
            double minutes = minutesNumerator / (double)minutesDenominator;

            uint secondsNumerator = BitConverter.ToUInt32(propertyItem.Value, 16);
            uint secondsDenominator = BitConverter.ToUInt32(propertyItem.Value, 20);
            double seconds = secondsNumerator / (double)secondsDenominator;

            double coorditate = degrees + (minutes / 60d) + (seconds / 3600d);
            //string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propertyItem.Value[0] }); //N, S, E, or W  

            if (gpsRef == 'S' || gpsRef == 'W')
            {
                coorditate *= -1;
            }

            return coorditate;
        }
    }
}
