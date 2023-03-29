using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public static class ImageResizeService
    {
        private const int OrientationKey = 0x0112;
        private const int NotSpecified = 0;
        private const int NormalOrientation = 1;
        private const int MirrorHorizontal = 2;
        private const int UpsideDown = 3;
        private const int MirrorVertical = 4;
        private const int MirrorHorizontalAndRotateRight = 5;
        private const int RotateLeft = 6;
        private const int MirorHorizontalAndRotateLeft = 7;
        private const int RotateRight = 8;

        /// <summary>
        /// Resize an image
        /// </summary>
        /// <param name="file">The original stream</param>
        /// <returns>The resize stream</returns>
        public static Stream GenerateNew(Stream file, float ratio = 1)
        {
            var imageStream = new MemoryStream();
            var image = Image.FromStream(file);

            if (ratio != 1)
            {
                var resized = ResizeImage(image, ratio);
                Rotate(image, resized);
                resized.Save(imageStream, ImageFormat.Jpeg);
            }
            else
            {
                image.Save(imageStream, ImageFormat.Jpeg);
            }

            imageStream.Seek(0, SeekOrigin.Begin);

            return imageStream;
        }

        public static Stream GenerateNew(Stream file, Size destinationSize)
        {
            var imageStream = new MemoryStream();
            var imgToResize = Image.FromStream(file);

            var sourceWidth = imgToResize.Width;
            var sourceHeight = imgToResize.Height;

            var nPercentW = (float)(destinationSize.Width / sourceWidth);
            var nPercentH = (float)(destinationSize.Height / sourceHeight);

            float nPercent;
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            var bitmap = ResizeImage(imgToResize, nPercent);
            Rotate(imgToResize, bitmap);
            bitmap.Save(imageStream, ImageFormat.Jpeg);
            imageStream.Seek(0, SeekOrigin.Begin);

            return imageStream;
        }


        public static Stream GenerateNew(Stream file, int thumbnaliWidth)
        {
            var imageStream = new MemoryStream();
            var imgToResize = Image.FromStream(file);

            
            var sourceWidth = imgToResize.Width;
            var sourceHeight = imgToResize.Height;

            var divisor = sourceWidth / thumbnaliWidth;
            if (divisor <= 0)
            {
                divisor = 1;
                thumbnaliWidth = sourceWidth;
            }

            var height = Convert.ToInt32(Math.Round((decimal)(sourceHeight / divisor)));

            var bitmap = ResizeImage(imgToResize, new Size(thumbnaliWidth, height));
            Rotate(imgToResize, bitmap);
            bitmap.Save(imageStream, ImageFormat.Jpeg);
            imageStream.Seek(0, SeekOrigin.Begin);

            return imageStream;
        }

        public static async Task<string> ToBase64(Stream file)
        {
            file.Seek(0, SeekOrigin.Begin);
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static bool IsLandscape(Stream imageFile)
        {
            var image = Image.FromStream(imageFile);
            return image.Width > image.Height;
        }

        public static DateTime? GetDateCreation(Stream imageFile)
        {
            var date = new DateTime?();
            var directories = ImageMetadataReader.ReadMetadata(imageFile);
            var exifSubDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            try
            {
                var originalDate = exifSubDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
                date = originalDate;
            }
            catch { }

            return date;
        }


        private static Bitmap ResizeImage(Image imgToResize, float ratio)
        {
            var sourceWidth = imgToResize.Width;
            var sourceHeight = imgToResize.Height;

            var destWidth = (int)(sourceWidth * ratio);
            var destHeight = (int)(sourceHeight * ratio);

            return new Bitmap(imgToResize, new Size(destWidth, destHeight));

        }

        private static Bitmap ResizeImage(Image imgToResize, Size newSize)
        {
            return new Bitmap(imgToResize, newSize);
        }

        private static void Rotate(Image image, Bitmap bitmap)
        {
            if (image.PropertyIdList.Contains(OrientationKey))
            {
                var orientation = (int)image.GetPropertyItem(OrientationKey).Value[0];
                switch (orientation)
                {
                    case NotSpecified: // Assume it is good.
                    case NormalOrientation:
                        // No rotation required.
                        break;
                    case MirrorHorizontal:
                        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case UpsideDown:
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case MirrorVertical:
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case MirrorHorizontalAndRotateRight:
                        bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case RotateLeft:
                        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case MirorHorizontalAndRotateLeft:
                        bitmap.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case RotateRight:
                        bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    default:
                        //throw new NotImplementedException("An orientation of " + orientation + " isn't implemented.");
                        break;
                }
            }
        }
    }
}
